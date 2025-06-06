// Em MipsProgramParser.cs ou Utils/MipsProgramParser.cs
using prototipo_conversor_assembly.Bases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit; // Não parece ser usado diretamente aqui, mas vou manter.
using System.Text.RegularExpressions; // Para parsing mais robusto

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class MipsProgramParser
    {
        public Dictionary<string, int> Labels { get; private set; } // Mapeamento de labels para endereços
        public List<MipsInstruction> ParsedInstructions { get; private set; } // Lista de instruções parseadas

        private BancoRegistradores _registerFile; // Referência para obter índices de registradores
        // private Dictionary<string, int> _labels = new Dictionary<string, int>(); // Esta linha é redundante, você já tem 'Labels' público. Vou remover.

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
                    Labels.Add(labelName, currentAddress); // Adiciona o rótulo com o endereço ATUAL

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
                    // ***** CORREÇÃO AQUI: Remove 'originalLine' do parâmetro *****
                    MipsInstruction instruction = ParseInstructionLine(lineToParse, currentAddress);
                    ParsedInstructions.Add(instruction);
                    currentAddress += 4;
                }
            }
        }

        // ***** CORREÇÃO AQUI: Altera a assinatura do método *****
        private MipsInstruction ParseInstructionLine(string instructionText, int address)
        {
            // Regex para capturar opcode e operandos de forma mais robusta
            Match match = Regex.Match(instructionText, @"^(\w+)\s*(.*)$"); // Usa instructionText
            if (!match.Success)
            {
                throw new FormatException($"Erro de formato na instrução: '{instructionText}'");
            }

            string opcode = match.Groups[1].Value.ToLower();
            string operandsPart = match.Groups[2].Value;

            string[] operands = operandsPart.Split(',').Select(op => op.Trim()).ToArray();

            switch (opcode)
            {
                case "addi": // rt, rs, immediate
                    {
                        string rtName = operands[0];
                        string rsName = operands[1];
                        int immediate;

                        string immediateString = operands[2];

                        if (immediateString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            immediate = Convert.ToInt32(immediateString.Substring(2), 16);
                        }
                        else
                        {
                            if (!int.TryParse(immediateString, out immediate))
                            {
                                throw new FormatException($"Formato de imediato inválido para addi: '{immediateString}'. Esperado um número decimal ou hexadecimal (0x...). Linha: '{instructionText}'"); // Usa instructionText
                            }
                        }

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new AddiInstruction(instructionText, address, rtIndex, rsIndex, immediate);
                    }
                case "add": // rd, rs, rt
                    {
                        string rdName = operands[0];
                        string rsName = operands[1];
                        string rtName = operands[2];

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new AddInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "sub": // rd, rs, rt
                    {
                        string rdName = operands[0];
                        string rsName = operands[1];
                        string rtName = operands[2];

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new SubInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "lw":
                    {
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido na instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        string rtName = memMatch.Groups[1].Value;
                        short offset = Convert.ToInt16(memMatch.Groups[2].Value);
                        string baseRegName = memMatch.Groups[3].Value;

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new LoadWordInstruction(instructionText, address, rtIndex, baseRegIndex, offset);
                    }
                case "sw":
                    {
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido na instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        string rtName = memMatch.Groups[1].Value;
                        short offset = Convert.ToInt16(memMatch.Groups[2].Value);
                        string baseRegName = memMatch.Groups[3].Value;

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new StoreWordInstruction(instructionText, address, rtIndex, baseRegIndex, offset);
                    }
                case "lh":
                    {
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido na instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        string rtName = memMatch.Groups[1].Value;
                        short offset = Convert.ToInt16(memMatch.Groups[2].Value);
                        string baseRegName = memMatch.Groups[3].Value;

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new LoadHalfInstruction(instructionText, address, rtIndex, baseRegIndex, offset);
                    }
                case "sh": // rt, offset(base)
                    {
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        string rtName = memMatch.Groups[1].Value;
                        short offset = Convert.ToInt16(memMatch.Groups[2].Value);
                        string baseRegName = memMatch.Groups[3].Value;

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new StoreHalfInstruction(instructionText, address, rtIndex, baseRegIndex, offset);
                    }
                case "lb": // rt, offset(base)
                    {
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        string rtName = memMatch.Groups[1].Value;
                        int offset = Convert.ToInt32(memMatch.Groups[2].Value);
                        string baseRegName = memMatch.Groups[3].Value;

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new LoadByteInstruction(instructionText, address, rtIndex, baseRegIndex, offset);
                    }
                case "sb": // rt, offset(base)
                    {
                        Match memMatch = Regex.Match(operandsPart, @"^([$]\w+\d*)\s*,\s*(-?\d+)\s*\(\s*([$]\w+\d*)\s*\)$");
                        if (!memMatch.Success)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
                        }

                        string rtName = memMatch.Groups[1].Value;
                        int offset = Convert.ToInt32(memMatch.Groups[2].Value);
                        string baseRegName = memMatch.Groups[3].Value;

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int baseRegIndex = _registerFile.GetRegisterIndex(baseRegName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new StoreByteInstruction(instructionText, address, rtIndex, baseRegIndex, offset);
                    }
                case "and": // rd, rs, rt
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new AndInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "or": // rd, rs, rt
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new OrInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "nor": // rd, rs, rt
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new NorInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
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

                        if (immediateString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            immediate = (int)Convert.ToUInt32(immediateString.Substring(2), 16);
                        }
                        else
                        {
                            if (!int.TryParse(immediateString, out immediate))
                            {
                                throw new FormatException($"Formato de imediato inválido para andi: '{immediateString}'. Esperado um número decimal ou hexadecimal (0x...). Linha: '{instructionText}'"); // Usa instructionText
                            }
                            immediate = (int)((uint)immediate & 0xFFFF);
                        }

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new AndiInstruction(instructionText, address, rtIndex, rsIndex, immediate);
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

                        if (immediateString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            immediate = (int)Convert.ToUInt32(immediateString.Substring(2), 16);
                        }
                        else
                        {
                            if (!int.TryParse(immediateString, out immediate))
                            {
                                throw new FormatException($"Formato de imediato inválido para ori: '{immediateString}'. Esperado um número decimal ou hexadecimal (0x...). Linha: '{instructionText}'"); // Usa instructionText
                            }
                            immediate = (int)((uint)immediate & 0xFFFF);
                        }

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new OriInstruction(instructionText, address, rtIndex, rsIndex, immediate);
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
                            throw new FormatException($"Valor de shamt inválido para sll: '{shamtString}'. Esperado um número decimal entre 0 e 31. Linha: '{instructionText}'"); // Usa instructionText
                        }

                        if (shamt < 0 || shamt > 31)
                        {
                            throw new ArgumentOutOfRangeException(nameof(shamt), $"Valor de shamt fora do intervalo permitido (0-31): {shamt}. Linha: '{instructionText}'"); // Usa instructionText
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new SllInstruction(instructionText, address, rdIndex, rtIndex, shamt);
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
                            throw new FormatException($"Valor de shamt inválido para srl: '{shamtString}'. Esperado um número decimal entre 0 e 31. Linha: '{instructionText}'"); // Usa instructionText
                        }

                        if (shamt < 0 || shamt > 31)
                        {
                            throw new ArgumentOutOfRangeException(nameof(shamt), $"Valor de shamt fora do intervalo permitido (0-31): {shamt}. Linha: '{instructionText}'"); // Usa instructionText
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new SrlInstruction(instructionText, address, rdIndex, rtIndex, shamt);
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
                        // ***** CORREÇÃO AQUI: Usa 'Labels' em vez de '_labels' *****
                        if (!Labels.TryGetValue(labelName, out int targetAddress))
                        {
                            throw new FormatException($"Rótulo '{labelName}' não encontrado ou indefinido. Linha: '{instructionText}'"); // Usa instructionText
                        }

                        // Calcular o offset
                        int offset = (targetAddress - (address + 4)) / 4;

                        // Verificar se o offset cabe em 16 bits (-32768 a 32767)
                        if (offset < short.MinValue || offset > short.MaxValue)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset para o rótulo '{labelName}' ({offset}) está fora do intervalo de 16 bits para beq. Linha: '{instructionText}'"); // Usa instructionText
                        }

                        // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                        return new BeqInstruction(instructionText, address, rsIndex, rtIndex, offset);
                    }
                case "bne": // rs, rt, label
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
                        if (!Labels.TryGetValue(labelName, out int targetAddress))
                        {
                            throw new FormatException($"Rótulo '{labelName}' não encontrado ou indefinido. Linha: '{instructionText}'");
                        }

                        // Calcular o offset da mesma forma que para beq
                        int offset = (targetAddress - (address + 4)) / 4;

                        // Verificar se o offset cabe em 16 bits
                        if (offset < short.MinValue || offset > short.MaxValue)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset para o rótulo '{labelName}' ({offset}) está fora do intervalo de 16 bits para bne. Linha: '{instructionText}'");
                        }

                        return new BneInstruction(instructionText, address, rsIndex, rtIndex, offset);
                    }
                case "slt": // rd, rs, rt (Set on Less Than)
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        string rdName = parts[0];
                        string rsName = parts[1];
                        string rtName = parts[2];

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        // Usa 'instructionText' no construtor
                        return new SltInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "sltu": // rd, rs, rt (Set on Less Than Unsigned)
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        string rdName = parts[0];
                        string rsName = parts[1];
                        string rtName = parts[2];

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        // Usa 'instructionText' no construtor
                        return new SltuInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "slti": // rt, rs, immediate (Set on Less Than Immediate)
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();

                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rt, $rs, immediate'");
                        }

                        string rtName = parts[0];
                        string rsName = parts[1];
                        string immediateString = parts[2];

                        short immediate;

                        // Parse do valor imediato (pode ser decimal ou hexadecimal)
                        if (immediateString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            // Convert.ToInt16 aceita strings hexadecimais para valores de 16 bits.
                            // Mas é mais seguro usar Convert.ToInt32 e então fazer o cast para short,
                            // garantindo que o valor caiba (mesmo que Int16 já fizesse isso).
                            int tempImmediate = Convert.ToInt32(immediateString.Substring(2), 16);
                            if (tempImmediate < short.MinValue || tempImmediate > short.MaxValue)
                            {
                                throw new FormatException($"Valor imediato '{immediateString}' fora do intervalo de 16 bits com sinal para SLTI. Linha: '{instructionText}'");
                            }
                            immediate = (short)tempImmediate;
                        }
                        else
                        {
                            if (!short.TryParse(immediateString, out immediate))
                            {
                                throw new FormatException($"Formato de imediato inválido para slti: '{immediateString}'. Esperado um número decimal ou hexadecimal (0x...). Linha: '{instructionText}'");
                            }
                        }

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        // Usa 'instructionText' no construtor
                        return new SltiInstruction(instructionText, address, rtIndex, rsIndex, immediate);
                    }
                case "j": // target (label) (Jump)
                    {
                        string labelName = operandsPart.Trim(); // O target é simplesmente o nome da label

                        if (string.IsNullOrWhiteSpace(labelName))
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado 'label'");
                        }

                        // Resolver o rótulo para obter o endereço de destino absoluto
                        if (!Labels.TryGetValue(labelName, out int targetAddress))
                        {
                            throw new FormatException($"Rótulo '{labelName}' não encontrado ou indefinido. Linha: '{instructionText}'");
                        }

                        // O endereço de destino do jump precisa ser alinhado à palavra (múltiplo de 4)
                        if (targetAddress % 4 != 0)
                        {
                            throw new FormatException($"Endereço de destino para jump '{labelName}' ({targetAddress:X}) não está alinhado à palavra. Linha: '{instructionText}'");
                        }

                        // Usa 'instructionText' no construtor
                        return new JumpInstruction(instructionText, address, targetAddress);
                    }
                case "jr": // rs (Jump Register)
                    {
                        string rsName = operandsPart.Trim(); // O único operando é o registrador

                        if (string.IsNullOrWhiteSpace(rsName))
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rs'");
                        }

                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        // Usa 'instructionText' no construtor
                        return new JumpRegisterInstruction(instructionText, address, rsIndex);
                    }
                case "jal": // target (label) (Jump And Link)
                    {
                        string labelName = operandsPart.Trim(); // O target é o nome da label

                        if (string.IsNullOrWhiteSpace(labelName))
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado 'label'");
                        }

                        // Resolver o rótulo para obter o endereço de destino absoluto
                        if (!Labels.TryGetValue(labelName, out int targetAddress))
                        {
                            throw new FormatException($"Rótulo '{labelName}' não encontrado ou indefinido. Linha: '{instructionText}'");
                        }

                        // O endereço de destino do jump precisa ser alinhado à palavra (múltiplo de 4)
                        if (targetAddress % 4 != 0)
                        {
                            throw new FormatException($"Endereço de destino para jal '{labelName}' ({targetAddress:X}) não está alinhado à palavra. Linha: '{instructionText}'");
                        }

                        // Usa 'instructionText' no construtor
                        return new JumpAndLinkInstruction(instructionText, address, targetAddress);
                    }
                // TODO: Adicionar cases para todas as outras instruções!

                default:
                    // ***** CORREÇÃO AQUI: Usa 'instructionText' no construtor *****
                    return new UnknownInstruction(instructionText, address, opcode); // Instrução não reconhecida
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