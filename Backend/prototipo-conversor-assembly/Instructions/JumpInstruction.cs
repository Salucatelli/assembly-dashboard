// Instructions/JumpInstruction.cs
using System;
using prototipo_conversor_assembly.Bases; // Mantenha se MipsInstruction estiver em prototipo_conversor_assembly.Bases

namespace prototipo_conversor_assembly
{
    public class JumpInstruction : MipsInstruction
    {
        // O target é o endereço COMPLETO para onde pular,
        // mas no MIPS, ele é um campo de 26 bits que representa um índice de palavra.
        // O parser fornecerá o endereço final já resolvido do rótulo.
        private int _targetAddress;

        public JumpInstruction(string assemblyLine, int address, int targetAddress)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.J; // J é uma instrução do tipo J
            _targetAddress = targetAddress;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // O PC é simplesmente definido para o endereço de destino.
            // Não há incremento de 4 aqui, pois o targetAddress já é o endereço absoluto.
            return _targetAddress;
        }

        public override string ToBinaryString()
        {
            // Formato J-Type: Opcode (6) | Target Address (26)
            // Opcode para J é 000010 (decimal 2)

            string opcodeBinary = Convert.ToString(0b000010, 2).PadLeft(6, '0'); // Opcode J

            // O target address na instrução J é o endereço de palavra (address / 4),
            // sem os 2 bits menos significativos (que são sempre 00 para endereços de palavras)
            // e sem os 4 bits mais significativos do PC.
            // Para converter o _targetAddress (que já é o endereço absoluto em bytes)
            // para o campo de 26 bits, fazemos: (targetAddress / 4)
            // E pegamos apenas os 26 bits menos significativos.
            uint targetWordAddress = (uint)_targetAddress / 4;
            string targetBinary = Convert.ToString(targetWordAddress, 2).PadLeft(26, '0'); // Garante 26 bits

            // Se o endereço for maior que o que cabe em 26 bits, pode haver truncamento.
            // Em um MIPS real, os 4 bits mais significativos do PC atual seriam pré-anexados.
            // Aqui, estamos simplificando, assumindo que o targetAddress se encaixa ou que o simulador
            // não precisará do endereço binário exato para ramificações fora do alcance de 26 bits de salto.
            // Para simulação, o _targetAddress já é o valor correto para onde pular.
            // Para o binário, pegamos os 26 bits, mas o cálculo do PC real envolve o PC + 4 e os bits do PC.
            // Neste simulador simples, o 'Execute' já usa o _targetAddress direto.

            // Um método mais preciso para o campo target de 26 bits seria:
            // (TargetAddress & 0x0FFFFFFF) >> 2
            // onde 0x0FFFFFFF remove os 4 bits superiores.
            // Para o binário, precisamos dos 26 bits inferiores desse resultado.

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