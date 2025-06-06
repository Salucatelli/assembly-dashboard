using System;
using prototipo_conversor_assembly.Bases; 

namespace prototipo_conversor_assembly
{
    public class JumpRegisterInstruction : MipsInstruction
    {
        private int _rsIndex; 

        public JumpRegisterInstruction(string assemblyLine, int address, int rsIndex)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.R; 
            _rsIndex = rsIndex;
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            int targetAddress = cpu.bancoDeRegistradores.GetValue(_rsIndex);

            if (targetAddress % 4 != 0)
            {
                throw new Exception($"Erro de alinhamento: Endereço de destino para JR ({targetAddress:X8}) não está alinhado à palavra (múltiplo de 4). Linha: '{AssemblyLine}'");
            }

            return targetAddress;
        }

        public override string ToBinaryString()
        {

            string opcodeBinary = Convert.ToString(0b000000, 2).PadLeft(6, '0'); 
            string rsBinary = Convert.ToString(_rsIndex, 2).PadLeft(5, '0');
            string rtBinary = Convert.ToString(0b00000, 2).PadLeft(5, '0'); 
            string rdBinary = Convert.ToString(0b00000, 2).PadLeft(5, '0'); 
            string shamtBinary = Convert.ToString(0b00000, 2).PadLeft(5, '0'); 
            string functBinary = Convert.ToString(0b001000, 2).PadLeft(6, '0'); 

            return $"{opcodeBinary}{rsBinary}{rtBinary}{rdBinary}{shamtBinary}{functBinary}";
        }

        public override string ToHexString()
        {
            uint binaryValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{binaryValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            return config.RTypeCycles;
        }
    }
}