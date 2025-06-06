# Simulador de Assembly MIPS em C#

## Visão Geral do Projeto

Este é um projeto desenvolvido para a disciplina de Aruqitetura de Computadores. Consiste em um simulador capaz de ler um arquivo txt com um código assembly do MIPS e detalhar o funcionamento da CPU, memórias e registradores a cada ciclo de clock.

## Funcionalidades Implementadas

O simulador atualmente suporta o parsing e a execução das seguintes instruções MIPS:

* **Aritméticas/Lógicas:**
    * `add` 
    * `addi` 
    * `sub` 
    * `and` 
    * `andi` 
    * `or` 
    * `ori` 
    * `nor` 
    * `sll` 
    * `srl` 
    * `slt` 
    * `sltu` 
    * `slti` 
    * `lw` 
    * `sw` 
    * `lh` 
    * `sh` 
    * `lb` 
    * `sb` 
    * `beq` 
    * `bne` 
    * `j` 
    * `jr` 
    * `jal` 

## Estrutura do Projeto

O projeto está organizado da seguinte maneira:

* `MipsProgramParser.cs`: Responsável por ler o arquivo Assembly MIPS, identificar as instruções e chamar a função da instrução certa.
* `MipsCPU.cs`: Representa a CPU do MIPS, contendo o Program Counter (PC), uma instância de `BancoRegistradores` e interagindo com a `MemoryMips`.
* `BancoRegistradores.cs`: Gerencia o estado dos 32 registradores presentes no MIPS.
* `MemoryMips.cs`: Simula a memória de dados do MIPS, permitindo operações de leitura e escrita.
* `Bases/MipsInstruction.cs`: Uma classe abstrata base que define a interface comum para todas as instruções MIPS (tipo, linha assembly original, endereço, método de execução, conversão binária/hexadecimal e ciclos de clock).
* `Instructions/`: Pasta contendo as implementações de cada uma das instruções do MIPS, sendo modeladas atravez da classe abstrata `MipsInstruction` .

## Como Usar

Para usar este simulador:

1.  **Clone o Repositório:**
    ```bash
    git clone [https://github.com/seu-usuario/seu-repositorio.git](https://github.com/seu-usuario/seu-repositorio.git)
    cd seu-repositorio
    ```
3.  **Código em Assembly:**
    Escreva um codigo em assembly do MIPS, e coloque-o em um arquivo chamado "assembly.txt", antes de executar o programa.

4.  **Executar:**
    Entre em uma pasta chamada "projeteo" e procure o arquivo "prototipo-conversor-assembly.exe". Execute-o para começaro programa.
