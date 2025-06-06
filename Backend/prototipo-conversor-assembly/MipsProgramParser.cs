// MipsProgramParser.cs
// Em MipsProgramParser.cs ou Utils/MipsProgramParser.cs
using prototipo_conversor_assembly.Bases;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Text.RegularExpressions; // Para parsing mais robusto
using System.Globalization; // Para InvariantCulture no double.Parse

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class MipsProgramParser
    {
        public Dictionary<string, int> Labels { get; private set; } 
        public List<MipsInstruction> ParsedInstructions { get; private set; } 

        private BancoRegistradores _registerFile; 
        public CpuConfig ProgramCpuConfig { get; private set; }


        public MipsProgramParser(BancoRegistradores registerFile)
        {
            Labels = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ParsedInstructions = new List<MipsInstruction>();
            _registerFile = registerFile;
            ProgramCpuConfig = new CpuConfig(); 
        }

        public void LoadAndParse(string filePath)
        {
            Labels.Clear();
            ParsedInstructions.Clear();
            ProgramCpuConfig = new CpuConfig(); 

            var lines = File.ReadAllLines(filePath);
            List<string> assemblyLinesForSecondPass = new List<string>(); 

            int currentAddress = 0; 

            for (int i = 0; i < lines.Length; i++)
            {
                string originalLine = lines[i];
                string trimmedLine = originalLine.Trim();

                if (trimmedLine.StartsWith("#CONFIG:", StringComparison.OrdinalIgnoreCase))
                {
                    ParseConfigLine(trimmedLine.Substring("#CONFIG:".Length).Trim());
                    continue; 
                }

                
                int commentIndex = trimmedLine.IndexOf('#');
                if (commentIndex != -1)
                {
                    trimmedLine = trimmedLine.Substring(0, commentIndex).Trim();
                }

                
                if (string.IsNullOrWhiteSpace(trimmedLine))
                {
                    continue;
                }

                
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

                    
                    string instructionPartAfterLabel = trimmedLine.Substring(labelSeparatorIndex + 1).Trim();

                    
                    if (!string.IsNullOrWhiteSpace(instructionPartAfterLabel))
                    {
                        assemblyLinesForSecondPass.Add(instructionPartAfterLabel); 
                        currentAddress += 4;
                    }
                    
                }
                else
                {
                    assemblyLinesForSecondPass.Add(trimmedLine); 
                    currentAddress += 4;
                }
            }

            currentAddress = 0; 
            foreach (var instructionText in assemblyLinesForSecondPass)
            {
                MipsInstruction instruction = ParseInstructionLine(instructionText, currentAddress);
                ParsedInstructions.Add(instruction);
                currentAddress += 4;
            }
        }

        private void ParseConfigLine(string configLine)
        {
            var parts = configLine.Split(new char[] { '=' }, 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                Console.WriteLine($"Aviso: Linha de configuração inválida: '{configLine}'. Esperado CHAVE=VALOR. Ignorando.");
                return;
            }

            string key = parts[0].Trim();
            string value = parts[1].Trim();

            try
            {
                switch (key)
                {
                    case "RTypeCycles":
                        ProgramCpuConfig.RTypeCycles = int.Parse(value);
                        break;
                    case "ITypeCycles":
                        ProgramCpuConfig.ITypeCycles = int.Parse(value);
                        break;
                    case "JTypeCycles":
                        ProgramCpuConfig.JTypeCycles = int.Parse(value);
                        break;
                    case "CpuClockMhz":
                        ProgramCpuConfig.ClockFrequencyMHz = double.Parse(value, CultureInfo.InvariantCulture);
                        break;
                    default:
                        Console.WriteLine($"Aviso: Chave de configuração desconhecida: '{key}'. Ignorando.");
                        break;
                }
            }
            catch (FormatException ex)
            {
                Console.WriteLine($"Erro de formato na configuração '{key}={value}': {ex.Message}. Usando valor padrão para '{key}'.");
            }
            catch (OverflowException ex)
            {
                Console.WriteLine($"Erro de estouro na configuração '{key}={value}': {ex.Message}. Usando valor padrão para '{key}'.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ocorreu um erro inesperado ao parsear a configuração '{key}={value}': {ex.Message}.");
            }
        }


        private MipsInstruction ParseInstructionLine(string instructionText, int address)
        {
            Match match = Regex.Match(instructionText, @"^(\w+)\s*(.*)$");
            if (!match.Success)
            {
                return new UnknownInstruction(instructionText, address, "INVALID_FORMAT");
            }

            string opcode = match.Groups[1].Value.ToLower();
            string operandsPart = match.Groups[2].Value.Trim();

            switch (opcode)
            {
                case "addi": 
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();
                        if (parts.Length != 3) throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rt, $rs, immediate'");

                        string rtName = parts[0];
                        string rsName = parts[1];
                        string immediateString = parts[2];

                        int immediate;
                        if (immediateString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
                            immediate = Convert.ToInt32(immediateString.Substring(2), 16);
                        }
                        else
                        {
                            if (!int.TryParse(immediateString, out immediate))
                            {
                                throw new FormatException($"Formato de imediato inválido para addi: '{immediateString}'. Esperado um número decimal ou hexadecimal (0x...). Linha: '{instructionText}'");
                            }
                        }

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        return new AddiInstruction(instructionText, address, rtIndex, rsIndex, immediate);
                    }
                case "add":
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();
                        if (parts.Length != 3) throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");

                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

                        return new AddInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "sub": 
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();
                        if (parts.Length != 3) throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");

                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

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

                        return new LoadHalfInstruction(instructionText, address, rtIndex, baseRegIndex, offset);
                    }
                case "sh": 
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

                        return new StoreHalfInstruction(instructionText, address, rtIndex, baseRegIndex, offset);
                    }
                case "lb": 
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

                        return new LoadByteInstruction(instructionText, address, rtIndex, baseRegIndex, offset);
                    }
                case "sb": 
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

                        return new StoreByteInstruction(instructionText, address, rtIndex, baseRegIndex, offset);
                    }
                case "and": 
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();
                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

                        return new AndInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "or": 
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();
                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

                        return new OrInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "nor": 
                    {
                        string[] parts = operandsPart.Split(',').Select(p => p.Trim()).ToArray();
                        if (parts.Length != 3)
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rd, $rs, $rt'");
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(parts[0]);
                        int rsIndex = _registerFile.GetRegisterIndex(parts[1]);
                        int rtIndex = _registerFile.GetRegisterIndex(parts[2]);

                        return new NorInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "andi": 
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
                                throw new FormatException($"Formato de imediato inválido para andi: '{immediateString}'. Esperado um número decimal ou hexadecimal (0x...). Linha: '{instructionText}'");
                            }
                            immediate = (int)((uint)immediate & 0xFFFF); 
                        }

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        return new AndiInstruction(instructionText, address, rtIndex, rsIndex, immediate);
                    }
                case "ori": 
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
                                throw new FormatException($"Formato de imediato inválido para ori: '{immediateString}'. Esperado um número decimal ou hexadecimal (0x...). Linha: '{instructionText}'");
                            }
                            immediate = (int)((uint)immediate & 0xFFFF); 
                        }

                        int rtIndex = _registerFile.GetRegisterIndex(rtName);
                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        return new OriInstruction(instructionText, address, rtIndex, rsIndex, immediate);
                    }
                case "sll": 
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
                            throw new FormatException($"Valor de shamt inválido para sll: '{shamtString}'. Esperado um número decimal entre 0 e 31. Linha: '{instructionText}'");
                        }

                        if (shamt < 0 || shamt > 31)
                        {
                            throw new ArgumentOutOfRangeException(nameof(shamt), $"Valor de shamt fora do intervalo permitido (0-31): {shamt}. Linha: '{instructionText}'");
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        return new SllInstruction(instructionText, address, rdIndex, rtIndex, shamt);
                    }
                case "srl": 
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
                            throw new FormatException($"Valor de shamt inválido para srl: '{shamtString}'. Esperado um número decimal entre 0 e 31. Linha: '{instructionText}'");
                        }

                        if (shamt < 0 || shamt > 31)
                        {
                            throw new ArgumentOutOfRangeException(nameof(shamt), $"Valor de shamt fora do intervalo permitido (0-31): {shamt}. Linha: '{instructionText}'");
                        }

                        int rdIndex = _registerFile.GetRegisterIndex(rdName);
                        int rtIndex = _registerFile.GetRegisterIndex(rtName);

                        return new SrlInstruction(instructionText, address, rdIndex, rtIndex, shamt);
                    }
                case "beq":
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

                        if (!Labels.TryGetValue(labelName, out int targetAddress))
                        {
                            throw new FormatException($"Rótulo '{labelName}' não encontrado ou indefinido. Linha: '{instructionText}'");
                        }

                        int offset = (targetAddress - (address + 4)) / 4;
                        if (offset < short.MinValue || offset > short.MaxValue)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset para o rótulo '{labelName}' ({offset}) está fora do intervalo de 16 bits para beq. Linha: '{instructionText}'");
                        }

                        return new BeqInstruction(instructionText, address, rsIndex, rtIndex, offset);
                    }
                case "bne": 
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

                        if (!Labels.TryGetValue(labelName, out int targetAddress))
                        {
                            throw new FormatException($"Rótulo '{labelName}' não encontrado ou indefinido. Linha: '{instructionText}'");
                        }

                        int offset = (targetAddress - (address + 4)) / 4;
                        if (offset < short.MinValue || offset > short.MaxValue)
                        {
                            throw new ArgumentOutOfRangeException(nameof(offset), $"Offset para o rótulo '{labelName}' ({offset}) está fora do intervalo de 16 bits para bne. Linha: '{instructionText}'");
                        }

                        return new BneInstruction(instructionText, address, rsIndex, rtIndex, offset);
                    }
                case "slt": 
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

                        return new SltInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "sltu": 
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

                        return new SltuInstruction(instructionText, address, rdIndex, rsIndex, rtIndex);
                    }
                case "slti": 
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
                        if (immediateString.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
                        {
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

                        return new SltiInstruction(instructionText, address, rtIndex, rsIndex, immediate);
                    }
                case "j": 
                    {
                        string labelName = operandsPart.Trim();
                        if (string.IsNullOrWhiteSpace(labelName))
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado 'label'");
                        }

                        if (!Labels.TryGetValue(labelName, out int targetAddress))
                        {
                            throw new FormatException($"Rótulo '{labelName}' não encontrado ou indefinido. Linha: '{instructionText}'");
                        }

                        if (targetAddress % 4 != 0)
                        {
                            throw new FormatException($"Endereço de destino para jump '{labelName}' ({targetAddress:X}) não está alinhado à palavra. Linha: '{instructionText}'");
                        }

                        return new JumpInstruction(instructionText, address, targetAddress);
                    }
                case "jr": 
                    {
                        string rsName = operandsPart.Trim();
                        if (string.IsNullOrWhiteSpace(rsName))
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado '$rs'");
                        }

                        int rsIndex = _registerFile.GetRegisterIndex(rsName);

                        return new JumpRegisterInstruction(instructionText, address, rsIndex);
                    }
                case "jal": 
                    {
                        string labelName = operandsPart.Trim();
                        if (string.IsNullOrWhiteSpace(labelName))
                        {
                            throw new FormatException($"Formato inválido para instrução '{opcode}': '{operandsPart}'. Esperado 'label'");
                        }

                        if (!Labels.TryGetValue(labelName, out int targetAddress))
                        {
                            throw new FormatException($"Rótulo '{labelName}' não encontrado ou indefinido. Linha: '{instructionText}'");
                        }

                        if (targetAddress % 4 != 0)
                        {
                            throw new FormatException($"Endereço de destino para jal '{labelName}' ({targetAddress:X}) não está alinhado à palavra. Linha: '{instructionText}'");
                        }

                        return new JumpAndLinkInstruction(instructionText, address, targetAddress);
                    }

                default:
                    return new UnknownInstruction(instructionText, address, opcode); 
            }
        }

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
                return cpu.pc + 4; 
            }

            public override string ToBinaryString() => "????????????????????????????????";
            public override string ToHexString() => "0x????????";
            public override int GetClockCycles(CpuConfig config) => 1; 
        }
    }
}