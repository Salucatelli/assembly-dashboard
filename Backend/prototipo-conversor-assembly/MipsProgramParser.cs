// Em MipsProgramParser.cs ou Utils/MipsProgramParser.cs
using prototipo_conversor_assembly.Bases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions; // Para parsing mais robusto

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class MipsProgramParser
    {
        public Dictionary<string, int> Labels { get; private set; } // Mapeamento de labels para endereços
        public List<MipsInstruction> ParsedInstructions { get; private set; } // Lista de instruções parseadas

        private BancoRegistradores _registerFile; // Referência para obter índices de registradores
        private Dictionary<string, int> _labels = new Dictionary<string, int>();

        public MipsProgramParser(BancoRegistradores registerFile)
        {
            Labels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ParsedInstructions = new List<MipsInstruction>();
            _registerFile = registerFile;
        }

        public void LoadAndParse(string filePath)
        {
            Labels.Clear();
            ParsedInstructions.Clear();

            var lines = File.ReadAllLines(filePath);
            int currentAddress = 0; // Endereço de memória MIPS, começa em 0x0 para simulação

            // --- Primeira passada: Identificar labels e seus endereços ---
            for (int i = 0; i < lines.Length; i++)
            {
                string originalLine = lines[i]; // Mantém a linha original para mensagens de erro
                string trimmedLine = originalLine.Trim();

                // 1. Remove comentários inline (#) primeiro
                int commentIndex = trimmedLine.IndexOf('#');
                if (commentIndex != -1)
                {
                    trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();
                }

                // 2. Ignora linhas que agora estão vazias
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue;
                }

                // 3. Verifica se a linha contém um rótulo
                int labelSeparatorIndex = trimmedLine.IndexOf(':');
                if (labelSeparatorIndex != -1)
                {
                    string labelName = trimmedLine.Substring(0, labelSeparatorIndex).Trim();
                    if (string.IsNullOrWhiteSpace(labelName))
                    {
                        throw new FormatException($"Erro: Rótulo malformado na linha {i + 1}: '{originalLine}'. Rótulos não podem ser vazios.");
                    }

                    if (Labels.ContainsKey(labelName))
                    {
                        throw new InvalidOperationException($"Erro: Rótulo '{labelName}' duplicado na linha {i + 1}.");
                    }
                    Labels.Add(labelName, currentAddress);

                    // Pega a parte da instrução após o rótulo
                    string instructionPartAfterLabel = trimmedLine.Substring(labelSeparatorIndex + 1).Trim();

                    // Se existe uma instrução após o rótulo, ela consome um endereço
                    if (!string.IsNullOrWhiteSpace(instructionPartAfterLabel))
                    {
                        currentAddress += 4;
                    }
                    // Se a linha era apenas "Label:", ela não consome endereço, e currentAddress não é incrementado
                }
                else
                {
                    // Se não há rótulo, a linha é uma instrução (ou era, e foi limpa de comentários/vazia)
                    // Se ela ainda não estiver vazia, significa que é uma instrução que consome endereço.
                    // (Esta condição já é coberta pelo 'if (string.IsNullOrWhiteSpace(trimmedLine)) continue;' no início)
                    currentAddress += 4;
                }
            }

            currentAddress = 0; // Reinicia o contador de endereço para a segunda passada

            // --- Segunda passada: Parsear instruções ---
            for (int i = 0; i < lines.Length; i++)
            {
                string originalLine = lines[i]; // Guarda a linha original para a propriedade AssemblyLine
                string lineToParse = originalLine.Trim();

                // 1. Remove comentários de linha (qualquer coisa após #)
                int commentIndex = lineToParse.IndexOf('#');
                if (commentIndex != -1)
                {
                    lineToParse = lineToParse.Substring(0, commentIndex).Trim();
                }

                // 2. Ignora linhas que agora estão vazias
                if (string.IsNullOrWhiteSpace(lineToParse))
                {
                    continue;
                }

                // 3. Remove a parte do rótulo se existir (para que 'ParseInstructionLine' receba apenas a instrução)
                int labelSeparatorIndex = lineToParse.IndexOf(':');
                if (labelSeparatorIndex != -1)
                {
                    string instructionPartAfterLabel = lineToParse.Substring(labelSeparatorIndex + 1).Trim();
                    if (string.IsNullOrWhiteSpace(instructionPartAfterLabel))
                    {
                        continue; // Era apenas um rótulo, não uma instrução nesta linha
                    }
                    lineToParse = instructionPartAfterLabel; // Agora 'lineToParse' é apenas a instrução
                }

                // 4. Se a linha ainda não estiver vazia, parse a instrução e adicione
                if (!string.IsNullOrWhiteSpace(lineToParse))
                {
                    MipsInstruction instruction = ParseInstructionLine(lineToParse, originalLine, currentAddress);
                    ParsedInstructions.Add(instruction);
                    currentAddress += 4;
                }
            }
        }

        private MipsInstruction ParseInstructionLine(string line, string originalLine, int address)
        {
            // Regex para capturar opcode e operandos de forma mais robusta
            // Permite espaços extras, vírgulas, parênteses para load/store
            // Ex: "addi $s1, 1, 2"
            // Ex: "lw $t0, 0($sp)"
            // Ex: "add $t2, $t0, $t1"

            // O padrão básico para instruções R, I, J (operandos separados por vírgula)
            Match match = Regex.Match(line, @"^(\w+)\s*(.*)$");
            if (!match.Success)
            {
                throw new FormatException($"Erro de formato na instrução: '{line}'");
            }

            string opcode = match.Groups[1].Value.ToLower(); // Ex: "addi"
            string operandsPart = match.Groups[2].Value; // Ex: "$s1,1,2" ou "$s2,$s1,4"

            string[] operands = operandsPart.Split(',').Select(op => op.Trim()).ToArray();

            switch (opcode)
            {
                case "addi": // rt, rs, immediate
                    {
                        string rtName = operands[0];
                        string rsName = operands[1];
                        int immediate = Convert.ToInt32(operands[2]); // short para 16 bits com sinal

                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();
                        string immediateString = parts[2];

                        // **** MUDAR AQUI ****
                        if (immediateString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            // Remove o "0x" e converte como hexadecimal (base 16)
                            immediate = Convert.ToInt32(immediateString.Substring(2), 16);
                        }
                        else
                        {
                            // Tenta converter como decimal. Pode ser positivo ou negativo.
                            // int.TryParse é mais robusto para números decimais.
                            if (!int.TryParse(immediateString, out immediate))
                            {
                                throw new FormatException($"Formato de imediato inválido para addi: '{immediateString}'. Esperado um número decimal ou hexadecimal (0x...). Linha: '{operandsPart}'");
                            }
                        }
                        // **** FIM DA MUDANÇA ****

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        return new AddiInstruction(originalLine, address, rtIndex, rsIndex, immediate);
                    }
                case "add": // rd, rs, rt
                    {
                        string rdName = operands[0];
                        string rsName = operands[1];
                        string rtName = operands[2];

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        return new AddInstruction(originalLine, address, rdIndex, rsIndex, rtIndex);
                    }
                case "sub": // rd, rs, rt
                    {
                        string rdName = operands[0];
                        string rsName = operands[1];
                        string rtName = operands[2];

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        return new SubInstruction(originalLine, address, rdIndex, rsIndex, rtIndex);
                    }
                case "lw":
                    {
                        // Regex específica para o formato de operandos de instruções de load/store: "$rt, offset($base)"
                        // Ex: "$t0, 0($sp)" ou "$s1,20($s2)"
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido na instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        // Captura os grupos da regex
                        string rtName = memMatch.Groups[1].Value;       // Ex: "$s1"
                        short offset = Convert.ToInt16(memMatch.Groups[2].Value); // Ex: "20"
                        string baseRegName = memMatch.Groups[3].Value;  // Ex: "$s2"

                        // Converte os nomes dos registradores para seus índices numéricos
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // Retorna uma nova instância da instrução LoadWordInstruction
                        return new LoadWordInstruction(originalLine, address, rtIndex, baseRegIndex, offset);
                    }
                case "sw":
                    {
                        // A mesma regex de lw serve para sw, pois o formato dos operandos é idêntico.
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido na instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        // Captura os grupos da regex
                        string rtName = memMatch.Groups[1].Value;       // Ex: "$s1" (registrador fonte para sw)
                        short offset = Convert.ToInt16(memMatch.Groups[2].Value); // Ex: "20"
                        string baseRegName = memMatch.Groups[3].Value;  // Ex: "$s2"

                        // Converte os nomes dos registradores para seus índices numéricos
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // Retorna uma nova instância da instrução StoreWordInstruction
                        return new StoreWordInstruction(originalLine, address, rtIndex, baseRegIndex, offset);
                    }
                case "lh":
                    {
                        // A mesma regex de lw e sw serve para lh, pois o formato dos operandos é idêntico.
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido na instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        // Captura os grupos da regex
                        string rtName = memMatch.Groups[1].Value;       // Ex: "$s1" (registrador destino para lh)
                        short offset = Convert.ToInt16(memMatch.Groups[2].Value); // Ex: "20"
                        string baseRegName = memMatch.Groups[3].Value;  // Ex: "$s2"

                        // Converte os nomes dos registradores para seus índices numéricos
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // Retorna uma nova instância da instrução LoadHalfInstruction
                        return new LoadHalfInstruction(originalLine, address, rtIndex, baseRegIndex, offset);
                    }
                case "sh": // rt, offset(base)
                    {
                        // A mesma regex de lw, sw, lh serve para sh, pois o formato dos operandos é idêntico.
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        // Captura os grupos da regex
                        string rtName = memMatch.Groups[1].Value;       // Registrador fonte para sh
                        short offset = Convert.ToInt16(memMatch.Groups[2].Value);
                        string baseRegName = memMatch.Groups[3].Value;

                        // Converte os nomes dos registradores para seus índices numéricos
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // Retorna uma nova instância da instrução StoreHalfInstruction
                        return new StoreHalfInstruction(originalLine, address, rtIndex, baseRegIndex, offset);
                    }
                case "lb": // rt, offset(base)
                    {
                        // A mesma regex de load/store é reutilizada.
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        // Captura os grupos da regex
                        string rtName = memMatch.Groups[1].Value;
                        // O offset é lido como int, conforme a nova diretriz
                        int offset = Convert.ToInt32(memMatch.Groups[2].Value);
                        string baseRegName = memMatch.Groups[3].Value;

                        // Converte os nomes dos registradores para seus índices numéricos
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // Retorna uma nova instância da instrução LoadByteInstruction
                        return new LoadByteInstruction(originalLine, address, rtIndex, baseRegIndex, offset);
                    }
                case "sb": // rt, offset(base)
                    {
                        // A mesma regex de load/store é reutilizada.
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        // Captura os grupos da regex
                        string rtName = memMatch.Groups[1].Value;
                        // O offset é lido como int
                        int offset = Convert.ToInt32(memMatch.Groups[2].Value);
                        string baseRegName = memMatch.Groups[3].Value;

                        // Converte os nomes dos registradores para seus índices numéricos
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // Retorna uma nova instância da instrução StoreByteInstruction
                        return new StoreByteInstruction(originalLine, address, rtIndex, baseRegIndex, offset);
                    }
                case "and": // rd, rs, rt
                    {
                        // Divide a string de operandos pela vírgula e remove espaços em branco
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        // Verifica se o número de operandos está correto
                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        // Converte os nomes dos registradores para seus índices numéricos
                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

                        // Retorna uma nova instância da instrução AndInstruction
                        return new AndInstruction(originalLine, address, rdIndex, rsIndex, rtIndex);
                    }
                case "or": // rd, rs, rt
                    {
                        // Divide a string de operandos pela vírgula e remove espaços em branco
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        // Verifica se o número de operandos está correto
                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        // Converte os nomes dos registradores para seus índices numéricos
                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

                        // Retorna uma nova instância da instrução OrInstruction
                        return new OrInstruction(originalLine, address, rdIndex, rsIndex, rtIndex);
                    }
                case "nor": // rd, rs, rt
                    {
                        // Divide a string de operandos pela vírgula e remove espaços em branco
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        // Verifica se o número de operandos está correto
                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        // Converte os nomes dos registradores para seus índices numéricos
                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

                        // Retorna uma nova instância da instrução NorInstruction
                        return new NorInstruction(originalLine, address, rdIndex, rsIndex, rtIndex);
                    }
                case "andi": // rt, rs, immediate
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rt, $rs, immediate'");
                        }

                        string rtName = parts[0];
                        string rsName = parts[1];
                        string immediateString = parts[2];

                        int immediate;

                        // Lógica para parsear imediato decimal ou hexadecimal
                        if (immediateString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            // Se o imediato for hexadecimal, remove o "0x" e converte para base 16
                            // Aqui o imediato pode ser considerado sem sinal, então usar Convert.ToUInt32 e depois cast para int
                            // para que o valor seja tratado como positivo.
                            immediate = (int)Convert.ToUInt32(immediateString.Substring(2), 16);
                        }
                        else
                        {
                            // Tenta converter como decimal.
                            // Para ANDI, é importante que o valor não seja negativo aqui,
                            // ou que você o trate como unsigned.
                            // Para manter a consistência, podemos ler como int e a instrução fará o zero-extend.
                            if (!int.TryParse(immediateString, out immediate))
                            {
                                throw new FormatException($"Formato de imediato inválido para andi: '{immediateString}'. Esperado um número decimal ou hexadecimal (0x...). Linha: '{originalLine}'");
                            }
                            // O MIPS trata o imediato de ANDI como unsigned de 16 bits.
                            // Se o usuário digitar um número negativo, ele será truncado e zero-estendido.
                            // Por exemplo, -1 será 0xFFFF (65535).
                            // Se precisar que -1 fique como -1, use ADDI.
                            // Para ANDI, o imediato é sempre um número entre 0 e 65535.
                            // Podemos forçar isso aqui:
                            if (immediate < 0 || immediate > 65535)
                            {
                                // É um número fora do range de 16 bits sem sinal.
                                // Podemos lançar um erro ou truncar. Truncar é o comportamento MIPS.
                                immediate = (int)((uint)immediate & 0xFFFF); // Trunca para 16 bits, tratando como unsigned
                            }
                        }

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        return new AndiInstruction(originalLine, address, rtIndex, rsIndex, immediate);
                    }
                case "ori": // rt, rs, immediate
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rt, $rs, immediate'");
                        }

                        string rtName = parts[0];
                        string rsName = parts[1];
                        string immediateString = parts[2];

                        int immediate;

                        // Lógica para parsear imediato decimal ou hexadecimal
                        if (immediateString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            // Se o imediato for hexadecimal, remove o "0x" e converte para base 16
                            // Usamos UInt32 e cast para int para tratar como valor sem sinal.
                            immediate = (int)Convert.ToUInt32(immediateString.Substring(2), 16);
                        }
                        else
                        {
                            // Tenta converter como decimal.
                            if (!int.TryParse(immediateString, out immediate))
                            {
                                throw new FormatException($"Formato de imediato inválido para ori: '{immediateString}'. Esperado um número decimal ou hexadecimal (0x...). Linha: '{originalLine}'");
                            }
                            // O MIPS trata o imediato de ORI como unsigned de 16 bits.
                            // Se o usuário digitar um número negativo, ele será truncado e zero-estendido.
                            // Para garantir que o valor esteja no range de 0 a 65535:
                            immediate = (int)((uint)immediate & 0xFFFF); // Trunca para 16 bits, tratando como unsigned
                        }

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        return new OriInstruction(originalLine, address, rtIndex, rsIndex, immediate);
                    }
                case "sll": // rd, rt, shamt
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rt, shamt'");
                        }

                        string rdName = parts[0];
                        string rtName = parts[1];
                        string shamtString = parts[2];

                        int shamt;
                        if (!int.TryParse(shamtString, out shamt))
                        {
                            throw new FormatException($"Valor de shamt inválido para sll: '{shamtString}'. Esperado um número decimal entre 0 e 31. Linha: '{originalLine}'");
                        }

                        // O shamt é um valor de 5 bits (0-31)
                        if (shamt < 0 || shamt > 31)
                        {
                            throw new ArgumentOutOfRangeException(nameof(shamt), $"Valor de shamt fora do intervalo permitido (0-31): {shamt}. Linha: '{originalLine}'");
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        return new SllInstruction(originalLine, address, rdIndex, rtIndex, shamt);
                    }
                case "srl": // rd, rt, shamt
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rt, shamt'");
                        }

                        string rdName = parts[0];
                        string rtName = parts[1];
                        string shamtString = parts[2];

                        int shamt;
                        if (!int.TryParse(shamtString, out shamt))
                        {
                            throw new FormatException($"Valor de shamt inválido para srl: '{shamtString}'. Esperado um número decimal entre 0 e 31. Linha: '{originalLine}'");
                        }

                        // O shamt é um valor de 5 bits (0-31)
                        if (shamt < 0 || shamt > 31)
                        {
                            throw new ArgumentOutOfRangeException(nameof(shamt), $"Valor de shamt fora do intervalo permitido (0-31): {shamt}. Linha: '{originalLine}'");
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        return new SrlInstruction(originalLine, address, rdIndex, rtIndex, shamt);
                    }
                case "beq": // rs, rt, label
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rs, $rt, label'");
                        }

                        string rsName = parts[0];
                        string rtName = parts[1];
                        string labelName = parts[2];

                        int rsIndex = _registerFile.GetRegisterIndex(rsName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        // Resolver o rótulo para obter o endereço de destino
                        if (!_labels.TryGetValue(labelName, out int targetAddress))
                        {
                            throw new FormatException($"Rótulo '{labelName}' não encontrado ou indefinido. Linha: '{originalLine}'");
                        }

                        // Calcular o offset
                        // O offset é relativo ao PC da instrução *seguinte* (address + 4)
                        // e é em número de PALAVRAS (dividir por 4).
                        int offset = (targetAddress - (address + 4)) / 4;

                        // Verificar se o offset cabe em 16 bits (-32768 a 32767)
                        if (offset < short.MinValue || offset > short.MaxValue)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset para o rótulo '{labelName}' ({offset}) está fora do intervalo de 16 bits para beq. Linha: '{originalLine}'");
                        }

                        return new BeqInstruction(originalLine, address, rsIndex, rtIndex, offset);
                    }
                // TODO: Adicionar cases para todas as outras instruções!
                // bne, slt, sltu, slti, j, jr, jal

                default:
                    return new UnknownInstruction(originalLine, address, opcode); // Instrução não reconhecida
            }
        }

        // Classe placeholder para instruções não implementadas/desconhecidas
        private class UnknownInstruction : MipsInstruction
        {
            private string _opcode;
            public UnknownInstruction(string assemblyLine, int address, string opcode) : base(assemblyLine, address)
            {
                Type = MipsInstructionType.Unknown;
                _opcode = opcode;
            }

            public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
            {
                Console.WriteLine($"Erro: Instrução '{_opcode}' não implementada ou desconhecida: {AssemblyLine}");
                return cpu.pc + 4; // Avança o PC para evitar loop infinito
            }

            public override string ToBinaryString() => "????????????????????????????????";
            public override string ToHexString() => "0x????????";
            public override int GetClockCycles(CpuConfig config) => 1; // Custo mínimo
        }
    }
}