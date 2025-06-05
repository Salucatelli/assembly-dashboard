using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace prototipo_conversor_assembly 
{
    public class CpuConfig
    {
        public double ClockFrequencyMHz { get; set; } // Frequência do clock em MHz
        public int RTypeCycles { get; set; }        // Ciclos para instruções Tipo R
        public int ITypeCycles { get; set; }        // Ciclos para instruções Tipo I
        public int JTypeCycles { get; set; }        // Ciclos para instruções Tipo J
        public int BranchCycles { get; set; } // Adicione esta linha (geralmente mais de 1 ciclo)

        public CpuConfig()
        {
            // Valores padrão
            ClockFrequencyMHz = 200; // 200 MHz
            RTypeCycles = 1;
            ITypeCycles = 1;
            JTypeCycles = 1;
            BranchCycles = 2;
        }
    }
}