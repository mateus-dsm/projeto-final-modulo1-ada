using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using Bogus.DataSets;
using CsvHelper.Configuration;
using CsvHelper;
using BCrypt;
using BCrypt.Net;
using static BCrypt.Net.BCrypt;
using System.Reflection.Metadata;
using System.Xml.Linq;

namespace AdaCredit.Domain
{

    public class Account
    {
        public string Name { get; set; }
        public long Document { get; set; }
        public bool ActiveAccount { get; set; }
        public string Number { get; set; }
        public string Branch { get; set; }
        public string BankCode { get; set; }
        public decimal Balance { get; set; }

        public Account(string name, long document, bool activeAccount, string number, string branch, string bankCode, decimal balance)
        {
            Name = name;
            Document = document;
            ActiveAccount = activeAccount;
            Number = number;
            Branch = branch;
            BankCode = bankCode;
            Balance = Math.Round(balance, 2);
        }

        public static string Hash(string cleanPassword)
        {
            var salt = GenerateSalt();
            return HashPassword(cleanPassword, salt);
        }
    }

    public class Transaction
    {
        public string TransferringBankCode { get; set; }
        public string TransferringBranch { get; set; }
        public string TransferringAccount { get; set; }
        public string ReceivingBankCode { get; set; }
        public string ReceivingBranch { get; set; }
        public string ReceivingAccount { get; set;}
        public string TransactionType { get; set;}
        public int TransactionDirection { get; set;}
        public decimal TransferredValue { get; set; }

        public Transaction(string transferringBankCode, string transferringBranch, string transferringAccount, string receivingBankCode, string receivingBranch, string receivingAccount, string transactionType, int transactionDirection, decimal transferredValue)
        {
            TransferringBankCode = transferringBankCode;
            TransferringBranch = transferringBranch;
            TransferringAccount = transferringAccount;
            ReceivingBankCode = receivingBankCode;
            ReceivingBranch = receivingBranch;
            ReceivingAccount = receivingAccount;
            TransactionType = transactionType;
            TransactionDirection = transactionDirection;
            TransferredValue = Math.Round(transferredValue, 2);
        }
    }

    public class FailedTransaction
    {
        public Transaction Data { get; set; }
        public string Reason { get; set; }

        public FailedTransaction(Transaction data, string reason)
        {
            Data = data;
            Reason = reason;
        }
    }

    public class Employee
    {
        public string Name { get; set; }
        public long Document { get; set; }
        public bool ActiveEmployee { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public DateTime LastLogin { get; set; }

        public Employee(string name, long document, bool activeEmployee, string user, string password, DateTime lastLogin)
        {
            Name = name;
            Document = document;
            ActiveEmployee = activeEmployee;
            User = user;
            Password = password;
            LastLogin = lastLogin;
        }
    }

    public static class LoginScreen
    {
        public static void FirstLogin(string path)
        {
            bool loggedIn = false;

            do
            {
                Console.Clear();

                Console.Write("Digite o nome de usuario: ");
                var username = Console.ReadLine();

                Console.Write("Digite a senha: ");
                var password = Console.ReadLine();

                if (username.Equals("user", StringComparison.InvariantCultureIgnoreCase) &&
                    password.Equals("pass", StringComparison.InvariantCultureIgnoreCase))
                    loggedIn = true;
                else
                {
                    Console.WriteLine("Usuario ou senha invalido! Pressione qualquer tecla para tentar novamente");
                    Console.ReadKey();
                }

            } while (!loggedIn);

            Console.Clear();
            Console.WriteLine("Cadastro inicial");
            Console.WriteLine("Por favor, insira suas informacoes abaixo: ");
            Console.WriteLine();

            Console.WriteLine("Nome completo:");
            string name = Console.ReadLine();

            Console.WriteLine("CPF (sem delimitadores):");
            long.TryParse(Console.ReadLine(), out long document);

            Console.Clear();
            Console.WriteLine("Cadastro inicial");
            Console.WriteLine("Agora defina seu login e senha:");
            Console.WriteLine();

            Console.WriteLine("Nome de usuario:");
            string user = Console.ReadLine();

            Console.WriteLine("Senha:");
            string pass = Account.Hash(Console.ReadLine());

            Employee newEmployee = new Employee(name, document, true, user, pass, DateTime.Now);

            using var streamWriter = new StreamWriter(path);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.WriteHeader<Employee>();
            csvWriter.NextRecord();
            csvWriter.WriteRecord(newEmployee);
        }

        public static void Login(string path)
        {
            bool loggedIn = false;

            while(!loggedIn)
            {
                List<Employee> employees = new List<Employee>();
                bool employeeFound = false;
                bool incorrectPassword = false;
                
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    PrepareHeaderForMatch = args => args.Header.ToUpper(),
                };

                using var streamReader = new StreamReader(path);
                using var csv = new CsvReader(streamReader, config);

                Console.Clear();
                Console.WriteLine("Insira suas informacoes para fazer o login");
                Console.WriteLine();

                Console.WriteLine("Nome de usuario:");
                string user = Console.ReadLine();

                Console.WriteLine("Senha:");
                string pass = Console.ReadLine();

                var records = csv.GetRecords<Employee>();
                foreach (var record in records)
                {
                    if (record.User == user)
                    {
                        employeeFound = true;
                        
                        if(Verify(pass, record.Password))
                        {
                            loggedIn = true;
                            record.LastLogin = DateTime.Now;
                        } else incorrectPassword = true;
                    }
                    employees.Add(record);
                }
                streamReader.Close();

                using var streamWriter = new StreamWriter(path);
                using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
                csvWriter.WriteHeader<Employee>();
                csvWriter.NextRecord();

                foreach (var employee in employees)
                {
                    if (employee.Document != 0)
                    {
                        csvWriter.WriteRecord(employee);
                        csvWriter.NextRecord();
                    }
                }

                if (!employeeFound)
                {
                    Console.WriteLine("Usuario nao encontrado!");
                    Console.WriteLine("Para digitar seu nome de usuario e senha novamente, aperte qualquer tecla");
                    Console.ReadKey();
                }
                else if (incorrectPassword)
                {
                    Console.WriteLine("Senha incorreta!");
                    Console.WriteLine("Para digitar seu nome de usuario e senha novamente, aperte qualquer tecla");
                    Console.ReadKey();
                }
            }
        }
    }

    public static class Menu
    {
        public static int Show()
        {
            Console.Clear();
            Console.WriteLine("Menu principal");
            Console.WriteLine("Digite um dos numeros abaixo para prosseguir para a pagina correspondente: ");
            Console.WriteLine();

            Console.WriteLine("0. Voltar para a tela de login");
            Console.WriteLine("1. Clientes");
            Console.WriteLine("2. Funcionarios");
            Console.WriteLine("3. Transacoes");
            Console.WriteLine("4. Relatorios");
            Console.WriteLine("5. Ajuda");

            Console.WriteLine();
            Console.WriteLine("Digite qualquer outro numero para sair do sistema");
            Console.WriteLine();

            int.TryParse(Console.ReadLine(), out int actionScreen);
            return actionScreen;
        }
    }

    public static class ClientsScreen
    {
        public static int Show()
        {
            Console.Clear();
            Console.WriteLine("Pagina de Clientes");
            Console.WriteLine("Digite um dos numeros abaixo para realizar a acao correspondente: ");
            Console.WriteLine();

            Console.WriteLine("1. Cadastrar novo cliente");
            Console.WriteLine("2. Consultar os dados de um cliente existente");
            Console.WriteLine("3. Alterar o cadastro de um cliente existente");
            Console.WriteLine("4. Desativar o cadastro de um cliente existente");

            Console.WriteLine();
            Console.WriteLine("Digite qualquer outro numero para sair da pagina");
            Console.WriteLine();

            int.TryParse(Console.ReadLine(), out int actionScreen);
            return actionScreen;
        }

        public static void CreateClientsFile(string path)
        {
            using var streamWriter = new StreamWriter(path);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.WriteHeader<Account>();
        }

        public static bool SignUpNewClient(string path)
        {
            Console.Clear();
            Console.WriteLine("Cadastrar novo cliente");
            Console.WriteLine();

            Console.WriteLine("Por favor, insira as informacoes do cliente abaixo:");
            Console.WriteLine();

            Console.WriteLine("Nome completo:");
            string name = Console.ReadLine();

            Console.WriteLine("CPF (sem delimitadores):");
            long.TryParse(Console.ReadLine(), out long document);

            Account newClientAccount = new Account(name, document, true, new Faker().Random.ReplaceNumbers("#####-#"), "0001", "777", 0);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };

            using var streamReader = new StreamReader(path);
            using var csv = new CsvParser(streamReader, config);

            while (csv.Read())
            {
                var line = csv.Record;
                string[] clientInformation = line.ToArray();
                long.TryParse(clientInformation[1], out long existingClientDocument);

                if (document == existingClientDocument)
                {
                    Console.WriteLine("CPF ja cadastrado!");
                    return Messages.NewActionMessage();
                }
            }
            streamReader.Close();

            using var stream = File.Open(path, FileMode.Append);
            using var streamWriter = new StreamWriter(stream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.NextRecord();
            csvWriter.WriteRecord(newClientAccount);

            Console.WriteLine("Novo cliente cadastrado com sucesso!");
            Console.WriteLine($"Numero da conta: {newClientAccount.Number}");
            Console.WriteLine($"Numero de agencia: {newClientAccount.Branch}");
            Console.WriteLine($"Codigo bancario: {newClientAccount.BankCode}");
            return Messages.NewActionMessage();
        }
        
        public static bool CheckExistingClientData(string path)
        {
            Console.Clear();
            Console.WriteLine("Consultar os dados de um cliente existente");
            Console.WriteLine();

            Console.WriteLine("Digite o CPF (sem delimitadores) do cliente:");
            long.TryParse(Console.ReadLine(), out long document);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };

            using var streamReader = new StreamReader(path);
            using var csv = new CsvParser(streamReader, config);

            while (csv.Read())
            {
                var line = csv.Record;
                string[] clientInformation = line.ToArray();
                long.TryParse(clientInformation[1], out long existingClientDocument);

                if (document == existingClientDocument)
                {
                    Console.WriteLine();
                    Console.WriteLine($"Nome do cliente: {clientInformation[0]}");
                    Console.WriteLine($"Cliente Ativo? {clientInformation[2]}");
                    Console.WriteLine($"Numero da conta: {clientInformation[3]}");
                    Console.WriteLine($"Numero da agencia: {clientInformation[4]}");
                    Console.WriteLine($"Codigo bancario: {clientInformation[5]}");
                    Console.WriteLine($"Saldo: R${clientInformation[6]}");
                    return Messages.NewActionMessage();
                }
            }
            streamReader.Close();

            Console.WriteLine("Cliente nao encontrado!");
            return Messages.NewActionMessage();
        }
        
        public static bool EditExistingClientAccount(string path)
        {
            Console.Clear();
            Console.WriteLine("Alterar o cadastro de um cliente existente");
            Console.WriteLine();

            Console.WriteLine("Digite o CPF (sem delimitadores) do cliente:");
            long.TryParse(Console.ReadLine(), out long document);

            List<Account> accounts = new List<Account>();
            bool clientFound = false;
            bool doNothing = false;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToUpper(),
            };

            using var streamReader = new StreamReader(path);
            using var csv = new CsvReader(streamReader, config);

            var records = csv.GetRecords<Account>();
            foreach (var record in records)
            {
                if (record.Document == document)
                {
                    clientFound = true;

                    Console.WriteLine();
                    Console.WriteLine("Digite um dos numeros abaixo para realizar a acao correspondente: ");
                    Console.WriteLine();

                    Console.WriteLine("1. Alterar o nome cadastrado");
                    Console.WriteLine("2. Alterar o CPF cadastrado");

                    Console.WriteLine();
                    Console.WriteLine("Digite qualquer outro numero para sair da pagina");
                    Console.WriteLine();

                    int.TryParse(Console.ReadLine(), out int actionScreen);
                    if(actionScreen == 1)
                    {
                        Console.WriteLine("Digite o novo nome: ");
                        record.Name = Console.ReadLine();
                    } 
                    else if (actionScreen == 2)
                    {
                        Console.WriteLine("Digite o novo CPF: ");
                        long.TryParse(Console.ReadLine(), out long newDocument);
                        record.Document = newDocument;
                    } else doNothing = true;
                }
                accounts.Add(record);
            }
            streamReader.Close();

            if (!clientFound)
            {
                Console.WriteLine("Cliente nao encontrado!");
                return Messages.NewActionMessage();
            }

            using var streamWriter = new StreamWriter(path);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.WriteHeader<Account>();
            csvWriter.NextRecord();

            foreach (var account in accounts)
            {
                if (account.Document != 0)
                {
                    csvWriter.WriteRecord(account);
                    csvWriter.NextRecord();
                }
            }

            if (!doNothing) Console.WriteLine("Dados alterados com sucesso!");
            return Messages.NewActionMessage();
        }
        
        public static bool TerminateExistingClientAccount(string path)
        {
            Console.Clear();
            Console.WriteLine("Desativar o cadastro de um cliente existente");
            Console.WriteLine();

            Console.WriteLine("Digite o CPF (sem delimitadores) do cliente:");
            long.TryParse(Console.ReadLine(), out long document);

            List<Account> accounts = new List<Account>();
            bool clientFound = false;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToUpper(),
            };
            
            using var streamReader = new StreamReader(path);
            using var csv = new CsvReader(streamReader, config);

            var records = csv.GetRecords<Account>();
            foreach (var record in records)
            {
                if (record.Document == document)
                {
                    clientFound = true;
                    record.ActiveAccount = false;
                }
                accounts.Add(record);
            }
            streamReader.Close();

            if (!clientFound)
            {
                Console.WriteLine("Cliente nao encontrado!");
                return Messages.NewActionMessage();
            }

            using var streamWriter = new StreamWriter(path);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.WriteHeader<Account>();
            csvWriter.NextRecord();

            foreach (var account in accounts)
            {
                if (account.Document != 0)
                {
                    csvWriter.WriteRecord(account);
                    csvWriter.NextRecord();
                }
            }

            Console.WriteLine("Cadastro desativado com sucesso!");
            return Messages.NewActionMessage();
        }
    }

    public static class EmployeesScreen
    {
        public static int Show()
        {
            Console.Clear();
            Console.WriteLine("Pagina de Funcionarios");
            Console.WriteLine("Digite um dos numeros abaixo para realizar a acao correspondente: ");
            Console.WriteLine();

            Console.WriteLine("1. Cadastrar novo funcionario");
            Console.WriteLine("2. Alterar a senha de um funcionario existente");
            Console.WriteLine("3. Desativar o cadastro de um funcionario existente");

            Console.WriteLine();
            Console.WriteLine("Digite qualquer outro numero para sair da pagina");
            Console.WriteLine();

            int.TryParse(Console.ReadLine(), out int actionScreen);
            return actionScreen;
        }

        public static bool SignUpNewEmployee(string path)
        {
            Console.Clear();
            Console.WriteLine("Cadastrar novo funcionario");
            Console.WriteLine();

            Console.WriteLine("Por favor, insira as informacoes do funcionario abaixo:");
            Console.WriteLine();

            Console.WriteLine("Nome completo:");
            string name = Console.ReadLine();

            Console.WriteLine("CPF (sem delimitadores):");
            long.TryParse(Console.ReadLine(), out long document);

            Console.WriteLine("Nome de usuario:");
            string user = Console.ReadLine();

            Console.WriteLine("Senha: ");
            string password = Account.Hash(Console.ReadLine());

            Employee newEmployee = new Employee(name, document, true, user, password, DateTime.Now);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
            };

            using var streamReader = new StreamReader(path);
            using var csv = new CsvParser(streamReader, config);

            while (csv.Read())
            {
                var line = csv.Record;
                string[] employeeInformation = line.ToArray();
                long.TryParse(employeeInformation[1], out long existingEmployeeDocument);

                if (document == existingEmployeeDocument)
                {
                    Console.WriteLine("CPF ja cadastrado!");
                    return Messages.NewActionMessage();
                }
            }
            streamReader.Close();

            using var stream = File.Open(path, FileMode.Append);
            using var streamWriter = new StreamWriter(stream);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.NextRecord();
            csvWriter.WriteRecord(newEmployee);

            Console.WriteLine("Novo funcionario cadastrado com sucesso!");
            return Messages.NewActionMessage();
        }

        public static bool EditExistingEmployeePassword(string path)
        {
            Console.Clear();
            Console.WriteLine("Alterar a senha de um funcionario existente");
            Console.WriteLine();

            Console.WriteLine("Digite o CPF (sem delimitadores) do funcionario:");
            long.TryParse(Console.ReadLine(), out long document);

            List<Employee> employees = new List<Employee>();
            bool employeeFound = false;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToUpper(),
            };

            using var streamReader = new StreamReader(path);
            using var csv = new CsvReader(streamReader, config);

            var records = csv.GetRecords<Employee>();
            foreach (var record in records)
            {
                if (record.Document == document)
                {
                    employeeFound = true;
                    Console.WriteLine();
                    Console.WriteLine("Digite a nova senha do funcionario:");
                    record.Password = Account.Hash(Console.ReadLine());
                }
                employees.Add(record);
            }
            streamReader.Close();

            if (!employeeFound)
            {
                Console.WriteLine("Funcionario nao encontrado!");
                return Messages.NewActionMessage();
            }

            using var streamWriter = new StreamWriter(path);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.WriteHeader<Employee>();
            csvWriter.NextRecord();

            foreach (var employee in employees)
            {
                if (employee.Document != 0)
                {
                    csvWriter.WriteRecord(employee);
                    csvWriter.NextRecord();
                }
            }

            Console.WriteLine("Senha alterada com sucesso!");
            return Messages.NewActionMessage();
        }

        public static bool TerminateExistingEmployeeAccount(string path)
        {
            Console.Clear();
            Console.WriteLine("Desativar o cadastro de um funcionario existente");
            Console.WriteLine();

            Console.WriteLine("Digite o CPF (sem delimitadores) do funcionario:");
            long.TryParse(Console.ReadLine(), out long document);

            List<Employee> employees = new List<Employee>();
            bool employeeFound = false;

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToUpper(),
            };

            using var streamReader = new StreamReader(path);
            using var csv = new CsvReader(streamReader, config);

            var records = csv.GetRecords<Employee>();
            foreach (var record in records)
            {
                if (record.Document == document)
                {
                    employeeFound = true;
                    record.ActiveEmployee = false;
                }
                employees.Add(record);
            }
            streamReader.Close();

            if (!employeeFound)
            {
                Console.WriteLine("Funcionario nao encontrado!");
                return Messages.NewActionMessage();
            }

            using var streamWriter = new StreamWriter(path);
            using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            csvWriter.WriteHeader<Employee>();
            csvWriter.NextRecord();

            foreach (var employee in employees)
            {
                if (employee.Document != 0)
                {
                    csvWriter.WriteRecord(employee);
                    csvWriter.NextRecord();
                }
            }

            Console.WriteLine("Cadastro desativado com sucesso!");
            return Messages.NewActionMessage();
        }
    }

    public static class TransactionsScreen
    {
        public static bool Show(string pathClients, string pathTransactions)
        {
            Console.Clear();

            string[] files = Directory.GetFiles(pathTransactions);

            foreach (string file in files)
            {
                List<Transaction> successfulTransactions = new List<Transaction>();
                List<FailedTransaction> failedTransactions = new List<FailedTransaction>();
                
                bool noBankFee = TransactionBeforeDecember(file);

                var configTransactionFiles = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false,
                };

                var configClientsFiles = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    PrepareHeaderForMatch = args => args.Header.ToUpper(),
                };

                using var streamReader = new StreamReader(file);
                using var csv = new CsvReader(streamReader, configTransactionFiles);

                var records = csv.GetRecords<Transaction>();
                foreach (var record in records)
                {
                    using var accountStreamReader = new StreamReader(pathClients);
                    using var accountCsv = new CsvReader(accountStreamReader, configClientsFiles);

                    var accountRecords = accountCsv.GetRecords<Account>();

                    List<Account> accounts = new List<Account>();
                    bool transactionFailed = true;
                    string failedTransactionReason = "Conta nao encontrada";

                    record.TransferringAccount = record.TransferringAccount.Insert(5, "-");
                    record.ReceivingAccount = record.ReceivingAccount.Insert(5, "-");

                    if (record.TransferringBankCode == "777" && record.ReceivingBankCode == "777")
                    {
                        bool transferringAccountFound = false;
                        bool receivingAccountFound = false;

                        foreach(var accountRecord in accountRecords)
                        {
                            if(accountRecord.Number == record.TransferringAccount)
                            {
                                if (accountRecord.ActiveAccount) transferringAccountFound = true;
                                else failedTransactionReason = "Conta inativa";

                                switch (record.TransactionDirection)
                                {
                                    case 0:
                                        // tira dinheiro

                                        accountRecord.Balance -= record.TransferredValue;

                                        if (!noBankFee)
                                        {
                                            // acrescenta a tarifa

                                            if (record.TransactionType == "TED")
                                            {
                                                accountRecord.Balance -= 5;
                                            }
                                            else if (record.TransactionType == "TEF")
                                            {
                                                // nao desconta
                                            }
                                            else if (record.TransactionType == "DOC")
                                            {
                                                if(record.TransferredValue < 500) accountRecord.Balance -= (1 + 0.01m*record.TransferredValue);
                                                else accountRecord.Balance -= 6;
                                            }
                                            else
                                            {
                                                transferringAccountFound = false;
                                                failedTransactionReason = "Tipo de transferencia invalido";
                                            }
                                        }

                                        if (accountRecord.Balance < 0)
                                        {
                                            transferringAccountFound = false;
                                            failedTransactionReason = "Saldo insuficiente";
                                        }

                                        break;
                                    case 1:
                                        // poe dinheiro

                                        accountRecord.Balance += record.TransferredValue;

                                        break;
                                }
                            }
                            else if(accountRecord.Number == record.ReceivingAccount)
                            {
                                if (accountRecord.ActiveAccount) receivingAccountFound = true;
                                else failedTransactionReason = "Conta inativa";

                                switch (record.TransactionDirection)
                                {
                                    case 0:
                                        // poe dinheiro

                                        accountRecord.Balance += record.TransferredValue;

                                        break;
                                    case 1:
                                        // tira dinheiro

                                        accountRecord.Balance -= record.TransferredValue;

                                        if (!noBankFee)
                                        {
                                            // acrescenta a tarifa

                                            if (record.TransactionType == "TED")
                                            {
                                                accountRecord.Balance -= 5;
                                            }
                                            else if (record.TransactionType == "TEF")
                                            {
                                                // nao desconta
                                            }
                                            else if (record.TransactionType == "DOC")
                                            {
                                                if (record.TransferredValue < 500) accountRecord.Balance -= (1 + 0.01m * record.TransferredValue);
                                                else accountRecord.Balance -= 6;
                                            }
                                            else
                                            {
                                                receivingAccountFound = false;
                                                failedTransactionReason = "Tipo de transferencia invalido";
                                            }
                                        }

                                        if (accountRecord.Balance < 0)
                                        {
                                            receivingAccountFound = false;
                                            failedTransactionReason = "Saldo insuficiente";
                                        }

                                        break;
                                }
                            }

                            accounts.Add(accountRecord);
                        }

                        if (transferringAccountFound && receivingAccountFound) transactionFailed = false;
                    }
                    else if(record.TransferringBankCode == "777")
                    {
                        foreach (var accountRecord in accountRecords)
                        {
                            if (accountRecord.Number == record.TransferringAccount)
                            {
                                //conta de origem
                                if (accountRecord.ActiveAccount) transactionFailed = false;
                                else failedTransactionReason = "Conta inativa";

                                switch (record.TransactionDirection)
                                {
                                    case 0:
                                        // tira dinheiro

                                        accountRecord.Balance -= record.TransferredValue;

                                        if (!noBankFee)
                                        {
                                            // acrescenta a tarifa

                                            if (record.TransactionType == "TED")
                                            {
                                                accountRecord.Balance -= 5;
                                            }
                                            else if (record.TransactionType == "DOC")
                                            {
                                                if (record.TransferredValue < 500) accountRecord.Balance -= (1 + 0.01m * record.TransferredValue);
                                                else accountRecord.Balance -= 6;
                                            }
                                            else
                                            {
                                                transactionFailed = true;
                                                failedTransactionReason = "Tipo de transferencia invalido";
                                            }
                                        }

                                        if (accountRecord.Balance < 0)
                                        {
                                            transactionFailed = true;
                                            failedTransactionReason = "Saldo insuficiente";
                                        }

                                        break;
                                    case 1:
                                        // poe dinheiro

                                        accountRecord.Balance += record.TransferredValue;

                                        if(!(record.TransactionType == "DOC" || record.TransactionType == "TED"))
                                        {
                                            transactionFailed = true;
                                            failedTransactionReason = "Tipo de transferencia invalido";
                                        }

                                        break;
                                }
                            }

                            accounts.Add(accountRecord);
                        }
                    }
                    else if(record.ReceivingBankCode == "777")
                    {
                        foreach (var accountRecord in accountRecords)
                        {
                            if (accountRecord.Number == record.ReceivingAccount)
                            {
                                //conta de destino
                                if (accountRecord.ActiveAccount) transactionFailed = false;
                                else failedTransactionReason = "Conta inativa";

                                switch (record.TransactionDirection)
                                {
                                    case 0:
                                        // poe dinheiro

                                        accountRecord.Balance += record.TransferredValue;

                                        if (!(record.TransactionType == "DOC" || record.TransactionType == "TED"))
                                        {
                                            transactionFailed = true;
                                            failedTransactionReason = "Tipo de transferencia invalido";
                                        }

                                        break;
                                    case 1:
                                        // tira dinheiro

                                        accountRecord.Balance -= record.TransferredValue;

                                        if (!noBankFee)
                                        {
                                            // acrescenta a tarifa

                                            if (record.TransactionType == "TED")
                                            {
                                                accountRecord.Balance -= 5;
                                            }
                                            else if (record.TransactionType == "DOC")
                                            {
                                                if (record.TransferredValue < 500) accountRecord.Balance -= (1 + 0.01m * record.TransferredValue);
                                                else accountRecord.Balance -= 6;
                                            }
                                            else
                                            {
                                                transactionFailed = true;
                                                failedTransactionReason = "Tipo de transferencia invalido";
                                            }
                                        }

                                        if (accountRecord.Balance < 0)
                                        {
                                            transactionFailed = true;
                                            failedTransactionReason = "Saldo insuficiente";
                                        }

                                        break;
                                }
                            }

                            accounts.Add(accountRecord);
                        }
                    }

                    accountStreamReader.Close();

                    if (transactionFailed)
                    {
                        FailedTransaction failedTransaction = new FailedTransaction(record, failedTransactionReason);
                        failedTransactions.Add(failedTransaction);
                        continue;
                    }
                    
                    successfulTransactions.Add(record);

                    using var streamWriter = new StreamWriter(pathClients);
                    using var csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
                    csvWriter.WriteHeader<Account>();
                    csvWriter.NextRecord();

                    foreach (var account in accounts)
                    {
                        if (account.Document != 0)
                        {
                            csvWriter.WriteRecord(account);
                            csvWriter.NextRecord();
                        }
                    }
                }

                if(successfulTransactions.Count() != 0)
                {
                    string transactionBankAndDate = file.Substring(0, file.Length-4);
                    string nameCompletedTransaction = transactionBankAndDate + "-completed.csv";
                    
                    using var completedTransactionsStreamWriter = new StreamWriter(InsertFolderInPath(nameCompletedTransaction, "Completed"));
                    using var completedTransactionsCsvWriter = new CsvWriter(completedTransactionsStreamWriter, CultureInfo.InvariantCulture);
                    completedTransactionsCsvWriter.WriteHeader<Transaction>();
                    completedTransactionsCsvWriter.NextRecord();

                    foreach(var transaction in successfulTransactions)
                    {
                        completedTransactionsCsvWriter.WriteRecord(transaction);
                        completedTransactionsCsvWriter.NextRecord();
                    }
                }

                if(failedTransactions.Count() != 0)
                {
                    string transactionBankAndDate = file.Substring(0, file.Length - 4);
                    string nameFailedTransaction = transactionBankAndDate + "-failed.csv";
                    
                    using var failedTransactionsStreamWriter = new StreamWriter(InsertFolderInPath(nameFailedTransaction, "Failed"));
                    using var failedTransactionsCsvWriter = new CsvWriter(failedTransactionsStreamWriter, CultureInfo.InvariantCulture);
                    failedTransactionsCsvWriter.WriteHeader<FailedTransaction>();
                    failedTransactionsCsvWriter.NextRecord();

                    foreach (var transaction in failedTransactions)
                    {
                        failedTransactionsCsvWriter.WriteRecord(transaction);
                        failedTransactionsCsvWriter.NextRecord();
                    }
                }

                streamReader.Close();
                File.Delete(file);
            }
            
            Console.WriteLine("Transacoes processadas com sucesso!");
            return Messages.NewActionMessage();
        }

        public static bool TransactionBeforeDecember(string fileName)
        {
            string[] nameArray = fileName.Split('-');
            string stringTransactionDate = nameArray[nameArray.Length - 1].Substring(0, 8);
            DateOnly transactionDate = DateOnly.ParseExact(stringTransactionDate, "yyyyMMdd", null);
            DateOnly limitDate = new DateOnly(2022, 12, 01);

            if (limitDate.CompareTo(transactionDate) > 0) return true;
            return false;
        }

        public static string InsertFolderInPath(string path, string folderName)
        {
            string[] nameArray = path.Split(@"\");
            string fileName = nameArray[nameArray.Length - 1];
            nameArray[nameArray.Length - 1] = folderName;
            string folderPath = String.Join(@"\", nameArray);
            return Path.Combine(folderPath, fileName);
        }
    }

    public static class ReportsScreen
    {
        public static int Show()
        {
            Console.Clear();
            Console.WriteLine("Pagina de Relatorios");
            Console.WriteLine("Digite um dos numeros abaixo para realizar a acao correspondente: ");
            Console.WriteLine();

            Console.WriteLine("1. Exibir todos os clientes ativos com seus respectivos saldos");
            Console.WriteLine("2. Exibir todos os clientes inativos");
            Console.WriteLine("3. Exibir todos os funcionarios ativos e sua ultima data e hora de login");
            Console.WriteLine("4. Exibir transacoes com erro");

            Console.WriteLine();
            Console.WriteLine("Digite qualquer outro numero para sair da pagina");
            Console.WriteLine();

            int.TryParse(Console.ReadLine(), out int actionScreen);
            return actionScreen;
        }
        
        public static bool ShowActiveClientsAndTheirBalances(string path)
        {
            Console.Clear();
            Console.WriteLine("Clientes Ativos:");
            Console.WriteLine();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToUpper(),
            };

            using var streamReader = new StreamReader(path);
            using var csv = new CsvReader(streamReader, config);

            var records = csv.GetRecords<Account>();
            foreach (var record in records)
            {
                if (record.ActiveAccount)
                {
                    Console.WriteLine($"Cliente: {record.Name}");
                    Console.WriteLine($"Numero da conta: {record.Number}");
                    Console.WriteLine($"Saldo: {record.Balance}");
                    Console.WriteLine();
                }
            }
            streamReader.Close();

            Console.WriteLine("Relatorio exibido com sucesso!");
            return Messages.NewActionMessage();
        }

        public static bool ShowInactiveClients(string path)
        {
            Console.Clear();
            Console.WriteLine("Clientes Inativos:");
            Console.WriteLine();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToUpper(),
            };

            using var streamReader = new StreamReader(path);
            using var csv = new CsvReader(streamReader, config);

            var records = csv.GetRecords<Account>();
            foreach (var record in records)
            {
                if (!record.ActiveAccount)
                {
                    Console.WriteLine($"Cliente: {record.Name}");
                    Console.WriteLine($"Numero da conta: {record.Number}");
                    Console.WriteLine();
                }
            }
            streamReader.Close();

            Console.WriteLine("Relatorio exibido com sucesso!");
            return Messages.NewActionMessage();
        }

        public static bool ShowActiveEmployees(string path)
        {
            Console.Clear();
            Console.WriteLine("Funcionarios Ativos:");
            Console.WriteLine();

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                PrepareHeaderForMatch = args => args.Header.ToUpper(),
            };

            using var streamReader = new StreamReader(path);
            using var csv = new CsvReader(streamReader, config);

            var records = csv.GetRecords<Employee>();
            foreach (var record in records)
            {
                if (record.ActiveEmployee)
                {
                    Console.WriteLine($"Funcionario: {record.Name}");
                    Console.WriteLine($"Nome de usuario: {record.User}");
                    Console.WriteLine($"Ultimo login: {record.LastLogin}");
                    Console.WriteLine();
                }
            }
            streamReader.Close();

            Console.WriteLine("Relatorio exibido com sucesso!");
            return Messages.NewActionMessage();
        }
        
        public static bool ShowUnsuccessfulTransactions(string path)
        {
            Console.Clear();
            Console.WriteLine("Transacoes com erro:");
            Console.WriteLine();

            string[] files = Directory.GetFiles(path);

            foreach (string file in files)
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = true,
                    PrepareHeaderForMatch = args => args.Header.ToUpper(),
                };

                using var streamReader = new StreamReader(file);
                using var csv = new CsvReader(streamReader, config);

                var records = csv.GetRecords<FailedTransaction>();
                foreach (var record in records)
                {
                    Console.WriteLine($"Codigo bancario da conta de origem: {record.Data.TransferringBankCode}");
                    Console.WriteLine($"Agencia da conta de origem: {record.Data.TransferringBranch}");
                    Console.WriteLine($"Conta de origem: {record.Data.TransferringAccount}");
                    Console.WriteLine($"Codigo bancario da conta de destino: {record.Data.ReceivingBankCode}");
                    Console.WriteLine($"Agencia da conta de destino: {record.Data.ReceivingBranch}");
                    Console.WriteLine($"Conta de destino: {record.Data.ReceivingAccount}");
                    Console.WriteLine($"Tipo da transacao: {record.Data.TransactionType}");
                    if(record.Data.TransactionDirection == 0)
                        Console.WriteLine("Sentido da transacao: Debito");
                    else if (record.Data.TransactionDirection == 1)
                        Console.WriteLine("Sentido da transacao: Credito");
                    Console.WriteLine($"Valor da transacao: R${record.Data.TransferredValue}");
                    Console.WriteLine($"Motivo do erro: {record.Reason}");
                    Console.WriteLine();
                }
                streamReader.Close();
            }

            Console.WriteLine("Relatorio exibido com sucesso!");
            return Messages.NewActionMessage();
        }
    }
    
    public static class Messages
    {
        public static void FirstMessage()
        {
            Console.WriteLine("Seja bem vindo ao sistema Ada Credit! Pressione qualquer tecla para continuar");
            Console.ReadKey();
        }

        public static bool NewActionMessage()
        {
            Console.WriteLine("Deseja realizar mais alguma acao? Aperte a tecla S " +
                "caso deseje e qualquer outra tecla caso contrario: ");
            if (Console.ReadKey().Key == ConsoleKey.S)
            {
                Console.WriteLine();
                return true;
            }
            else
            {
                Console.WriteLine();
                return false;
            }
        }

        public static void HelpMessage()
        {
            Console.Clear();
            Console.WriteLine("Paginas do sistema Ada Credit e suas opcoes: ");
            Console.WriteLine();
            Console.WriteLine("1. Clientes: Cadastrar novo cliente, Consultar os dados de um cliente existente, " +
                "Alterar o cadastro de um cliente existente, Desativar o cadastro de um cliente existente");
            Console.WriteLine("2. Funcionarios: Cadastrar novo funcionario, Alterar a senha de um funcionario " +
                "existente, Desativar o cadastro de um funcionario existente");
            Console.WriteLine("3. Transacoes: Processar transacoes (Reconciliacao bancaria)");
            Console.WriteLine("4. Relatorios: Exibir todos os clientes ativos com seus respectivos saldos, " +
                "Exibir todos os clientes inativos, Exibir todos os funcionarios ativos e sua ultima data e " +
                "hora de login, Exibir transacoes com erro (Detalhes da transacao e do erro)");
            Console.WriteLine();
            Console.WriteLine("Pressione qualquer tecla para sair");
            Console.ReadKey();
        }

        public static void EndMessage()
        {
            Console.WriteLine("Obrigado por usar o sistema Ada Credit! Pressione qualquer tecla para fechar a aplicacao");
            Console.ReadKey();
        }
    }
}
