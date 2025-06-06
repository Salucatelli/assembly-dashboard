// Instructions/JumpAndLinkInstruction.cs
using System;
using prototipo_conversor_assembly.Bases; // Mantenha se MipsInstruction estiver em prototipo_conversor_assembly.Bases

namespace prototipo_conversor_assembly
{
    public class JumpAndLinkInstruction : MipsInstruction
    {
        // O target é o endereço COMPLETO para onde pular,
        // mas no MIPS, ele é um campo de 26 bits que representa um índice de palavra.
        // O parser fornecerá o endereço final já resolvido do rótulo.
        private int _targetAddress;

        // O índice do registrador $ra (Return Address) é 31 em MIPS
        private const int RA_REGISTER_INDEX = 31;

        public JumpAndLinkInstruction(string assemblyLine, int address, int targetAddress)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.J; // JAL é uma instrução do tipo J
            _targetAddress = targetAddress;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // 1. Salvar o endereço de retorno:
            // O endereço de retorno é o endereço da *próxima instrução* após o JAL.
            // Em MIPS, o PC é incrementado em 4 para buscar a próxima instrução *antes* da execução.
            // No caso de um JAL, o endereço a ser salvo em $ra é (PC atual da instrução + 8).
            // Isso ocorre porque após o JAL, o PC é atualizado para o endereço de salto,
            // então o "próximo" endereço sequencial seria Address + 4, e a instrução "depois dessa"
            // (que seria a que seria executada sequencialmente se não houvesse o jump)
            // é Address + 8.
            int returnAddress = Address + 8;

            // Salva o endereço de retorno no registrador $ra (registrador 31)
            // Usando seu padrão 'cpu.bancoDeRegistradores.SetValue()'
            cpu.bancoDeRegistradores.SetValue(RA_REGISTER_INDEX, returnAddress);

            // 2. Realizar o salto:
            // O PC é definido para o endereço de destino.
            return _targetAddress;
        }

        public override string ToBinaryString()
        {
            // Formato J-Type: Opcode (6) | Target Address (26)
            // Opcode para JAL é 000011 (decimal 3)

            string opcodeBinary = Convert.ToString(0b000011, 2).PadLeft(6, '0'); // Opcode JAL

            // O target address na instrução JAL é o endereço de palavra (address / 4),
            // sem os 2 bits menos significativos (que são sempre 00 para endereços de palavras)
            // e sem os 4 bits mais significativos do PC.
            // Para converter o _targetAddress (que já é o endereço absoluto em bytes)
            // para o campo de 26 bits, fazemos: (targetAddress / 4)
            // E pegamos apenas os 26 bits menos significativos.

            uint adjustedTarget = ((uint)_targetAddress & 0x0FFFFFFF) >> 2; // Pega os 26 bits (ignora os 4 superiores e os 2 inferiores)
            string adjustedTargetBinary = Convert.ToString(adjustedTarget, 2).PadLeft(26, '0');

            return $"{opcodeBinary}{adjustedTargetBinary}";
        }

        public override string ToHexString()
        {
            // Converte a string binária para um inteiro sem sinal e depois para hexadecimal
            uint binaryValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{binaryValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            // O custo em ciclos de clock para uma instrução do tipo J
            return config.JTypeCycles;
        }
    }
}