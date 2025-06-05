// Instructions/BeqInstruction.cs
using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly // Seu namespace atual
{
    public class BeqInstruction : MipsInstruction
    {
        private int _rsIndex;   // Primeiro registrador para comparação
        private int _rtIndex;   // Segundo registrador para comparação
        private int _offset;    // Offset (pode ser int, mas representa 16 bits sign-extended)

        // O labelName é útil para o parser, mas não precisamos dele na instrução em si para execução
        // Pois o parser já o traduziu para um offset.
        // Se você quiser mantê-lo para fins de depuração ou exibição, pode adicioná-lo.
        // private string _labelName;

        public BeqInstruction(string assemblyLine, int address, int rsIndex, int rtIndex, int offset)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I; // Beq é I-Type
            _rsIndex = rsIndex;
            _rtIndex = rtIndex;
            _offset = offset;
            // _labelName = labelName; // Se você adicionar o parâmetro ao construtor
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            // 1. Obter os valores dos registradores a serem comparados
            int rsValue = cpu.bancoDeRegistradores.GetValue(_rsIndex);
            int rtValue = cpu.bancoDeRegistradores.GetValue(_rtIndex);

            // 2. Determinar o endereço da próxima instrução (PC + 4)
            // Assumimos que 'cpu.pc' aqui já é o endereço desta instrução.
            // O próximo PC sequencial SEMPRE seria cpu.pc + 4.
            int nextPc = cpu.pc + 4;

            // 3. Comparar os valores dos registradores
            if (rsValue == rtValue)
            {
                // 4. Calcular o endereço de desvio
                // O offset é em palavras de 4 bytes, então multiplicamos por 4.
                // O offset é sign-extended de 16 bits para 32 bits antes da multiplicação.
                // Como _offset já é um 'int' (32 bits), a multiplicação já acontece com o valor correto.
                int branchTargetAddress = nextPc + (_offset * 4);

                // 5. Retornar o novo endereço do PC
                return branchTargetAddress;
            }
            else
            {
                // 6. Se não forem iguais, continuar a execução sequencial
                return nextPc;
            }
        }

        public override string ToBinaryString()
        {
            // Formato I-Type: opcode (6) | rs (5) | rt (5) | immediate (16)
            // Opcode para BEQ é 000100 (4 em decimal)
            string opcode = "000100";
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(_rtIndex, 2).PadLeft(5, '0');

            // O offset (imediato) é de 16 bits COM SINAL.
            // Usamos (short)_offset para garantir a representação de 16 bits e
            // preenchimento de sinal ('1' para negativos, '0' para positivos).
            string offsetBinary = Convert.ToString((short)_offset, 2).PadLeft(16, (_offset < 0 ? '1' : '0'));

            return $"{opcode}{rsBinary}{rtBinary}{offsetBinary}";
        }

        public override string ToHexString()
        {
            uint instructionValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{instructionValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config) => config.BranchCycles; // Assumindo que você tem um ciclo para branches
    }
}