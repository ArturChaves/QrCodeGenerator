# README — Encoder de QR Code (C#/.NET) sem libs + Componente MAUI

Este README explica detalhadamente o funcionamento do **encoder completo de QR Code** implementado manualmente (sem bibliotecas externas) e do **componente MAUI reutilizável (`QrView`)** para renderização do QR.

---

## 📦 Estrutura do Projeto

```
Qr/
 ├─ Core/
 │   ├─ BitBuffer.cs
 │   ├─ GaloisField.cs
 │   ├─ ReedSolomon.cs
 │   └─ Tables.cs
 ├─ Encoding/
 │   ├─ DataEncoderByte.cs
 │   ├─ Interleaver.cs
 │   └─ Masking.cs
 ├─ Matrix/
 │   ├─ TemplateBuilder.cs
 │   ├─ DataPlacer.cs
 │   └─ FormatVersionWriter.cs
 └─ QrEncoder.cs

App/Controls/
 └─ QrView.xaml(.cs)
```

Cada módulo é independente e responsável por uma etapa do pipeline de geração do QR. Isso permite fácil depuração e substituição de partes específicas (por exemplo, outro modo de codificação ou nível ECC diferente).

---

## 🧬 Pipeline de Geração do QR

1. **Escolha de versão:** o encoder calcula quantos bytes a mensagem ocupa e escolhe a menor versão (v1..v40) que a suporta no nível ECC M.
2. **Codificação dos dados:** adiciona o cabeçalho de modo (BYTE → `0100`), o comprimento e os bytes da mensagem.
3. **Padding:** adiciona bits terminadores e bytes de preenchimento (`0xEC`, `0x11`) até preencher a capacidade de dados.
4. **Blocagem e ECC:** divide os dados conforme a tabela ECC da versão e gera os blocos ECC via Reed–Solomon.
5. **Interleaving:** embaralha todos os blocos de dados e ECC para formar a sequência final de codewords.
6. **Template:** constrói a matriz base com todos os padrões fixos (finders, alignment, timing etc.).
7. **Placement:** insere os bits dos codewords na matriz em zigue-zague, pulando as áreas funcionais.
8. **Máscara:** aplica as 8 máscaras possíveis, calcula a penalidade e escolhe a com menor pontuação.
9. **Formato e Versão:** escreve os bits BCH com o nível ECC e máscara escolhidos.
10. **Saída:** gera uma matriz `bool[,]` representando os módulos pretos e brancos do QR.

---

## ⚙️ Explicação de Cada Arquivo

### 🧩 Core/

#### **BitBuffer.cs**
Gerencia a manipulação de bits e a conversão para bytes.  
Funções principais:
- `Append(val, length)`: adiciona bits MSB → LSB.  
- `PadToCodewords(total)`: aplica terminador, alinhamento e bytes de preenchimento.  
- `ToCodewords()`: converte a sequência em bytes (inteiros 0–255).

#### **GaloisField.cs**
Implementa o corpo finito GF(256) com polinômio primitivo `0x11D`.  
Usado no cálculo de ECC Reed–Solomon.

#### **ReedSolomon.cs**
Gera os blocos de redundância (ECC).  
Etapas:
- Cria o polinômio gerador com base no número de símbolos ECC.  
- Divide o polinômio de dados e pega o resto (símbolos ECC).  
- Retorna o vetor final de ECC.

#### **Tables.cs**
Guarda as tabelas oficiais do padrão QR:  
- `EccTableM`: especificações dos blocos por versão.  
- `ALIGNMENT`: posições dos centros de alinhamento.  
- Também define utilitários como `VersionSize(ver)` (n = 17 + 4v).

---

### 🧠 Encoding/

#### **DataEncoderByte.cs**
Codifica mensagens no **modo BYTE** (4 bits `0100`).  
Adiciona o contador de caracteres (8 ou 16 bits) e todos os bytes ISO-8859-1.  
> Pode ser adaptado para UTF-8 com ECI (valor 26).

#### **Interleaver.cs**
Gera o fluxo final de codewords.  
- Divide em blocos conforme a tabela ECC.  
- Gera ECC via Reed–Solomon.  
- Intercala bytes de todos os blocos (DATA, depois ECC).  
> A ordem é crítica para que o leitor interprete corretamente.

#### **Masking.cs**
Aplica as máscaras padrão (0–7) e calcula penalidades:
- **N1**: sequências longas iguais.  
- **N2**: blocos 2×2 iguais.  
- **N3**: padrões 1:1:3:1:1.  
- **N4**: proporção global de pretos (ideal 50%).  
A máscara com menor penalidade é escolhida.

---

### 🧩 Matrix/

#### **TemplateBuilder.cs**
Cria a estrutura base da matriz QR:
- Finders (3 cantos), Timing patterns, Alinhamentos (dependendo da versão).  
- Reservas para bits de formato e versão.  
- Módulo escuro fixo (dark module).  
Marca cada célula funcional com `func[r,c]=true` e preenche o resto com placeholders (`2`).

#### **DataPlacer.cs**
Insere os bits de dados na matriz:
- Percorre a matriz em colunas pares (duas por vez), indo para cima e para baixo alternadamente.  
- Pula a coluna 6 (timing).  
- Escreve bits onde `func[r,c]==false`.

#### **FormatVersionWriter.cs**
Escreve bits de **FORMATO** e **VERSÃO** com BCH.
- **Formato:** 15 bits `(ECL + MASK + BCH + XOR 0x5412)`.  
- **Versão:** 18 bits `(ver + BCH)`, usados a partir da v7.  
Grava nas posições exatas definidas pelo padrão.

---

### 🧩 QrEncoder.cs

Coordena todas as etapas:
1. Calcula o tamanho da mensagem e escolhe versão.  
2. Codifica os dados e faz padding.  
3. Gera blocos ECC e faz interleaving.  
4. Cria o template, faz o placement e aplica máscaras.  
5. Escreve bits de formato e versão.  
6. Retorna a matriz `bool[,]`.

> **Saída final:** `true` = módulo preto; `false` = branco.

---

## 🎨 Componente .NET MAUI — `QrView`

O componente `QrView` usa `GraphicsView` para desenhar o QR no Canvas.
Ele consome apenas a matriz `bool[,]` e permite ampla customização visual.

### ✨ Propriedades

| Propriedade | Tipo | Descrição |
|--------------|------|------------|
| **Text** | string | Conteúdo codificado no QR. |
| **ForegroundColor** | Color | Cor dos módulos. (Padrão: preto) |
| **BackgroundColor2** | Color | Cor de fundo do QR (dentro da área). |
| **QuietZone** | int | Margem em módulos (recomendado ≥ 4). |
| **FitToAvailable** | bool | Ajusta automaticamente o QR para caber na área. |
| **ModuleSize** | double | Tamanho fixo do módulo (em px) se `FitToAvailable=false`. |
| **PixelPerfect** | bool | Arredonda a escala para inteiro e evita borrões. |

### 💡 Uso Básico

```xml
xmlns:controls="clr-namespace:App.Controls"

<controls:QrView Text="https://api.seuservico.com/v1/mobile/activate?token=AAAA..."
                 ForegroundColor="#222"
                 BackgroundColor2="#FAFAFA"
                 QuietZone="6"
                 WidthRequest="280"
                 HeightRequest="280" />
```

### 📏 Uso com Tamanho Fixo

```xml
<controls:QrView Text="HELLO WORLD"
                 FitToAvailable="False"
                 ModuleSize="10"
                 PixelPerfect="True"
                 ForegroundColor="DarkBlue"
                 BackgroundColor2="White"
                 WidthRequest="400"
                 HeightRequest="400" />
```

---

## 📖 Conceitos-Chave

### Modos e Cabeçalhos
- **Modo BYTE:** prefixo `0100`.  
- **Contagem:** 8 bits (v1–v9) / 16 bits (v10–v40).  
- **Encoding:** ISO-8859-1 (ou UTF-8 se adicionado ECI 26).

### ECC Reed–Solomon (Nível M)
- Trabalha em GF(256).  
- Cada bloco tem `EcPerBlock` bytes de correção.  
- Permite recuperar ~15% de perda de dados.

### Máscaras e Penalidades
- 8 padrões aplicados sobre os módulos de dados.  
- Penalidades N1..N4 determinam a melhor máscara.

### Bits de Formato e Versão
- **Formato:** 15 bits (nível ECC + máscara + BCH).  
- **Versão:** 18 bits (v≥7).  
- Gravados em posições fixas para que o leitor identifique a versão e correção.

---

## 🧩 Boas Práticas

- **Quiet Zone ≥ 4**: scanners esperam essa margem branca.  
- **Cores contrastantes:** evite pares de cores similares (p.ex., cinza sobre azul).  
- **Escala inteira:** evita borrões de interpolação.  
- **Mensagens longas:** use versões ≥ 20 e mantenha contraste alto.  
- **URLs:** encode espaços com `%20` antes de gerar.
