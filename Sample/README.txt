Arquivo Settings.ini deve ser distribuído junto com o arquivo Update.exe

O arquivo Settings.ini contém informação sobre onde está disponibilizada a última versão de cada arquivo a ser atualizado e possui o seguinte formato:
[Update]
UpdateUrl=FilesLocation

Este local informado no arquivo Settings.ini deve conter obrigatoriamente:

	Um arquivo chamado Files.txt:
		Listagem dos arquivos disponíveis no servidor, cada linha do arquivo representa um arquivo a ser baixado do servidor, e possui o seguinte formato:

		MD5 FileName FileSize

	Um arquivo chamado Modules.xml:
		Este arquivo possui a estrutura do programa a ser baixado/atualizado.

		Este arquivo possui a seguinte estrutura:
		<Modules>
			Listagem de downloads disponíveis:
			<Download EnabledByDefault="True" Module="Nome do módulo"/>
			A opçao EnabledByDefault é considerada true quando não informada.


			Listagem de módulos disponíveis:
			<Module Name="Nome do módulo" Zip="Módulo.zip">
				Lista de dependências do módulo:
				<Dependency Module="Nome de outro módulo"/>
				Esta listagem é opcional.

				Listagem de arquivos do módulo:
				<File Path="ArquivoExemplo.txt"/>
				Estes arquivos devem estar descritos também no arquivo Files.txt

			</Module>
			O atributo Zip é opcional, e quando informada, deve indicar um arquivo zip que contenha todos os arquivos do modulo.

		</Modules>

	O servidor precisa também conter os arquivos descritos no arquivo Files.txt


Quando executado o arquivo Update.exe, serão atualizados os arquivos dos módulos que estão indicados pela tag Download.
Será baixado o arquivo Zip sempre que disponível e o mesmo possua um tamanho menor do que o necessário para atualizar os arquivos do módulo.
