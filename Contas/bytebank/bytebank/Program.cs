using bytebank.Contas;
using bytebank.Titular;


ContaCorrente conta = new ContaCorrente(18, "1010-X");
conta.SetSaldo(500);

conta.Titular = new Cliente();

Console.WriteLine(conta.GetSaldo());
Console.WriteLine(conta.Numero_agencia);
Console.WriteLine(conta.Conta);



ContaCorrente conta5 = new ContaCorrente(283, "1234-X");
Console.WriteLine(ContaCorrente.TotalDeContasCriadas);


ContaCorrente conta6 = new ContaCorrente(284, "4321-X");
Console.WriteLine(ContaCorrente.TotalDeContasCriadas);

ContaCorrente conta7 = new ContaCorrente(285, "4321-R");
Console.WriteLine(ContaCorrente.TotalDeContasCriadas);

Cliente sarah = new Cliente();
sarah.Nome = "Sarah Silva";
sarah.Profissao = "Professora";
sarah.Cpf = "11111111-12";

Cliente ester = new Cliente();
ester.Nome = "Ester Almeida";
ester.Profissao = "Advogada";
ester.Cpf = "868524125-32";

Console.WriteLine("Total de clientes: " + Cliente.TotalClientesCadastrados);