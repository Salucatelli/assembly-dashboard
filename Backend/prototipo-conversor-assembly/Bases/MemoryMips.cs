using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prototipo_conversor_assembly.Bases
{
    public class MemoryMips
    {
        private byte[] _data;
        private const int WordSize = 4; // 4 bytes por palavra MIPS

        public int SizeInBytes => _data.Length;

        public MemoryMips(int sizeInBytes)
        {
            if (sizeInBytes <= 0 || sizeInBytes % WordSize != 0)
            {
                throw new ArgumentException("Memory size must be a positive multiple of 4 bytes.");
            }
            _data = new byte[sizeInBytes];
        }

        public int ReadWord(int address)
        {
            if (address < 0 || address + WordSize > _data.Length || address % WordSize != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(address), $"Address 0x{address:X8} out of bounds or not word-aligned.");
            }
            return BitConverter.ToInt32(_data, address);
        }

        public void WriteWord(int address, int value)
        {
            if (address < 0 || address + WordSize > _data.Length || address % WordSize != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(address), $"Address 0x{address:X8} out of bounds or not word-aligned.");
            }
            Array.Copy(BitConverter.GetBytes(value), 0, _data, address, WordSize);
        }

        public short ReadHalf(int address)
        {
            if (address < 0 || address + 2 > _data.Length || address % 2 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(address), $"Address 0x{address:X8} out of bounds or not half-word-aligned.");
            }
            return BitConverter.ToInt16(_data, address);
        }

        public void WriteHalf(int address, short value)
        {
            if (address < 0 || address + 2 > _data.Length || address % 2 != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(address), $"Address 0x{address:X8} out of bounds or not half-word-aligned.");
            }
            Array.Copy(BitConverter.GetBytes(value), 0, _data, address, 2);
        }

        public byte ReadByte(int address)
        {
            if (address < 0 || address >= _data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(address), $"Address 0x{address:X8} out of bounds.");
            }
            return _data[address];
        }

        public void WriteByte(int address, byte value)
        {
            if (address < 0 || address >= _data.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(address), $"Address 0x{address:X8} out of bounds.");
            }
            _data[address] = value;
        }

        public void Reset()
        {
            Array.Clear(_data, 0, _data.Length); // Limpa a memória, preenchendo com zeros
        }
    }
}
