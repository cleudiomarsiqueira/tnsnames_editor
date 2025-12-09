# TNS Names Editor

Uma aplicação Windows Forms para editar e gerenciar arquivos `tnsnames.ora` do Oracle.

## Estrutura do Projeto

```
TnsNamesEditor_projeto/
├── TnsNamesEditor.sln              # Arquivo da solução
├── .gitignore                      # Configurações do Git
├── TnsNamesEditor/                 # Projeto principal
│   ├── Program.cs                  # Ponto de entrada da aplicação
│   ├── TnsNamesEditor.csproj       # Configurações do projeto
│   ├── Forms/                      # Formulários da interface
│   │   ├── MainForm.cs/Designer.cs/resx
│   │   ├── EditEntryForm.cs/Designer.cs/resx
│   │   └── PasteTnsForm.cs/Designer.cs
│   ├── Models/                     # Modelos de dados
│   │   └── TnsNamesParser.cs       # Parser de arquivos TNS
│   ├── Resources/                  # Arquivos de recursos
│   │   └── tnsnames_exemplo.ora    # Exemplo de arquivo TNS
│   ├── bin/                        # Binários compilados
│   └── obj/                        # Arquivos de objeto
├── docs/                           # Documentação do projeto
│   ├── README.md
│   ├── CHANGELOG.md
│   ├── RESUMO_PROJETO.md
│   ├── GUIA_RAPIDO.md
│   └── ...
└── samples/                        # Exemplos e testes
    └── test_parse.txt
```

## Padrões de Organização

- **Forms/**: Todos os formulários Windows Forms
  - Cada formulário tem: `.cs`, `.Designer.cs`, `.resx`
  - Namespace: `TnsNamesEditor.Forms`

- **Models/**: Modelos de dados e parsers
  - Namespace: `TnsNamesEditor.Models`

- **Resources/**: Arquivos de recursos como dados de exemplo

- **docs/**: Documentação em Markdown

- **samples/**: Exemplos e arquivos de teste

## Compilação

```powershell
dotnet build TnsNamesEditor.sln
```

## Execução

```powershell
dotnet run --project TnsNamesEditor/TnsNamesEditor.csproj
```

## Publish

```powershell
cd TnsNamesEditor
.\publish.ps1
```

## Requisitos

- .NET 8.0 ou superior
- Windows (aplicação Windows Forms)
