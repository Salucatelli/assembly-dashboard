// Em MipsProgramParser.cs ou Utils/MipsProgramParser.cs
using prototipo_conversor_assembly.Bases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions; // Para parsing mais robusto

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class MipsProgramParser
    {
        public Dictionary<string, int> Labels { get; private set; } // Mapeamento de labels para endereços
        public List<MipsInstruction> ParsedInstructions { get; private set; } // Lista de instruções parseadas

        private BancoRegistradores _registerFile; // Referência para obter índices de registradores

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

            // Primeira passada: Identificar labels e seus endereços
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue; // Ignora linhas vazias ou comentários
                }

                // Verifica se é uma label
                if (line.EndsWith(":"))
                {
                    string labelName = line.Substring(0, line.Length - 1);
                    if (Labels.ContainsKey(labelName))
                    {
                        throw new InvalidOperationException($"Erro: Label '{labelName}' duplicada na linha {i + 1}.");
                    }
                    Labels.Add(labelName, currentAddress);
                    continue; // Labels não consomem espaço de instrução
                }
                // Se a linha contém uma instrução após uma label (ex: Label: addi $t0, $t1, 5)
                else if (line.Contains(":"))
                {
                    string[] parts = line.Split(':', 2);
                    string labelName = parts[0].Trim();
                    string instructionPart = parts[1].Trim();

                    if (Labels.ContainsKey(labelName))
                    {
                        throw new InvalidOperationException($"Erro: Label '{labelName}' duplicada na linha {i + 1}.");
                    }
                    Labels.Add(labelName, currentAddress);
                    // Agora processa a parte da instrução
                    // Não continue, pois esta linha ainda tem uma instrução para processar na segunda passada
                }

                // Se chegou até aqui, é uma instrução, então avança o endereço
                currentAddress += 4; // Cada instrução MIPS ocupa 4 bytes
            }

            currentAddress = 0; // Reinicia o contador de endereço para a segunda passada

            // Segunda passada: Parsear instruções
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                string originalLine = lines[i]; // Guarda a linha original para a propriedade AssemblyLine

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                {
                    continue;
                }

                // Remove comentários de linha (qualquer coisa após #)
                int commentIndex = line.IndexOf('#');
                if (commentIndex != -1)
                {
                    line = line.Substring(0, commentIndex).Trim();
                }

                if (string.IsNullOrWhiteSpace(line)) continue; // Se só tinha comentário

                // Remove labels se existirem na mesma linha da instrução
                if (line.Contains(":"))
                {
                    line = line.Split(':', 2)[1].Trim();
                }

                if (string.IsNullOrWhiteSpace(line)) continue; // Se só tinha label

                // Agora, parse a instrução
                MipsInstruction instruction = ParseInstructionLine(line, originalLine, currentAddress);
                ParsedInstructions.Add(instruction);
                currentAddress += 4;
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
                        short immediate = Convert.ToInt16(operands[2]); // short para 16 bits com sinal

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
                            throw new FormatException($"Formato inválido para instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
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
                            throw new FormatException($"Formato inválido para instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
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
                            throw new FormatException($"Formato inválido para instrução '{opcode}': {operandsPart}. Esperado '$rt, offset($base)'");
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
                // TODO: Adicionar cases para todas as outras instruções!
                // lw, sw, lh, sh, lb, sb, and, or, nor, andi, ori, sll, srl, beq, bne, slt, sltu, slti, j, jr, jal

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