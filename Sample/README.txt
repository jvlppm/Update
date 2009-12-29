Arquivo Settings.ini deve ser distribu�do junto com o arquivo Update.exe

O arquivo Settings.ini cont�m informa��o sobre onde est� disponibilizada a �ltima vers�o de cada arquivo a ser atualizado e possui o seguinte formato:
[Update]
UpdateUrl=FilesLocation

Este local informado no arquivo Settings.ini deve conter obrigatoriamente:

	Um arquivo chamado Files.txt:
		Listagem dos arquivos dispon�veis no servidor, cada linha do arquivo representa um arquivo a ser baixado do servidor, e possui o seguinte formato:

		MD5 FileName FileSize

	Um arquivo chamado Modules.xml:
		Este arquivo possui a estrutura do programa a ser baixado/atualizado.

		Este arquivo possui a seguinte estrutura:
		<Modules>
			Listagem de downloads dispon�veis:
			<Download EnabledByDefault="True" Module="Nome do m�dulo"/>
			A op�ao EnabledByDefault � considerada true quando n�o informada.


			Listagem de m�dulos dispon�veis:
			<Module Name="Nome do m�dulo" Zip="M�dulo.zip">
				Lista de depend�ncias do m�dulo:
				<Dependency Module="Nome de outro m�dulo"/>
				Esta listagem � opcional.

				Listagem de arquivos do m�dulo:
				<File Path="ArquivoExemplo.txt"/>
				Estes arquivos devem estar descritos tamb�m no arquivo Files.txt

			</Module>
			O atributo Zip � opcional, e quando informada, deve indicar um arquivo zip que contenha todos os arquivos do modulo.

		</Modules>

	O servidor precisa tamb�m conter os arquivos descritos no arquivo Files.txt


Quando executado o arquivo Update.exe, ser�o atualizados os arquivos dos m�dulos que est�o indicados pela tag Download.
Ser� baixado o arquivo Zip sempre que dispon�vel e o mesmo possua um tamanho menor do que o necess�rio para atualizar os arquivos do m�dulo.
