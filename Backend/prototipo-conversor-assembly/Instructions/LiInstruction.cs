using prototipo_conversor_assembly.Bases;
using System;

namespace prototipo_conversor_assembly
{
    public class LiInstruction : MipsInstruction
    {
        public int RdIndex { get; private set; }      
        public int ImmediateValue { get; private set; } 

        private readonly byte _opcodeForBinary; 
        private readonly byte _functionForBinary; 

        private readonly bool _isAddiTranslation;
        private readonly int _luiUpperImmediate;
        private readonly int _oriLowerImmediate;


        public LiInstruction(string assemblyLine, int address, int rdIndex, int immediateValue)
            : base(assemblyLine, address)
        {
            Type = MipsInstructionType.I; 

            RdIndex = rdIndex;
            ImmediateValue = immediateValue;

            if (immediateValue >= short.MinValue && immediateValue <= short.MaxValue)
            {
                _isAddiTranslation = true;
                _opcodeForBinary = 0x08; 
                _functionForBinary = 0; 
            }
            else
            {
                _isAddiTranslation = false;
                _opcodeForBinary = 0x0F; 
                _functionForBinary = 0; 

                _luiUpperImmediate = (int)((uint)immediateValue >> 16);
                _oriLowerImmediate = (int)((uint)immediateValue & 0x0000FFFF);
            }
        }

        public override int Execute(MipsCPU cpu, MemoryMips dataMemory)
        {
            cpu.bancoDeRegistradores.SetValue(RdIndex, ImmediateValue);

            return cpu.pc + 4; 
        }

        public override string ToBinaryString()
        {
            if (_isAddiTranslation)
            {
                string opcodeBin = Convert.ToString(_opcodeForBinary, 2).PadLeft(6, '0');
                string rsBin = Convert.ToString(0, 2).PadLeft(5, '0'); 
                string rtBin = Convert.ToString(RdIndex, 2).PadLeft(5, '0');
                string immediateBin = Convert.ToString(ImmediateValue & 0xFFFF, 2).PadLeft(16, '0');

                return $"{opcodeBin}{rsBin}{rtBin}{immediateBin}";
            }
            else
            {
                string opcodeBin = Convert.ToString(_opcodeForBinary, 2).PadLeft(6, '0');
                string rsBin = Convert.ToString(0, 2).PadLeft(5, '0'); 
                string rtBin = Convert.ToString(1, 2).PadLeft(5, '0'); 
                string immediateBin = Convert.ToString(_luiUpperImmediate & 0xFFFF, 2).PadLeft(16, '0'); 

                return $"{opcodeBin}{rsBin}{rtBin}{immediateBin}";
            }
        }

        public override string ToHexString()
        {
            uint binaryValue = Convert.ToUInt32(ToBinaryString(), 2);
            return $"0x{binaryValue:X8}";
        }

        public override int GetClockCycles(CpuConfig config)
        {
            return _isAddiTranslation ? config.ITypeCycles : (config.ITypeCycles * 2);
        }
    }
}