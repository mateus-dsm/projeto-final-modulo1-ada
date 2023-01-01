using System;
using System.ComponentModel.Design;
using AdaCredit.Domain;

namespace AdaCredit
{
    internal class Program
    {
        static void Main(string[] args)
        {
            bool closeProgram = false;
            int idScreen;
            int actionScreen;

            string pathDesktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string pathEmployeesFile = Path.Combine(pathDesktop, "users.csv");
            string pathClientsFile = Path.Combine(pathDesktop, "clients.csv");
            string pathTransactionsFolder = Path.Combine(pathDesktop, "Transactions");
            string pathCompletedTransactionsFolder = Path.Combine(pathTransactionsFolder, "Completed");
            string pathFailedTransactionsFolder = Path.Combine(pathTransactionsFolder, "Failed");

            Messages.FirstMessage();

            if (!File.Exists(pathEmployeesFile))
            {
                LoginScreen.FirstLogin(pathEmployeesFile);
                idScreen = 5;
            }
            else idScreen = 0;

            if (!File.Exists(pathClientsFile))
            {
                ClientsScreen.CreateClientsFile(pathClientsFile);
            }
            
            if (!Directory.Exists(pathTransactionsFolder))
            {
                Directory.CreateDirectory(pathTransactionsFolder);
                Directory.CreateDirectory(pathCompletedTransactionsFolder);
                Directory.CreateDirectory(pathFailedTransactionsFolder);
            }
            else
            {
                if(!Directory.Exists(pathCompletedTransactionsFolder))
                    Directory.CreateDirectory(pathCompletedTransactionsFolder);
                if(!Directory.Exists(pathFailedTransactionsFolder))
                    Directory.CreateDirectory(pathFailedTransactionsFolder);
            }
            
            while(!closeProgram)
            {
                /*
                 * ID das telas:
                 * 0: Login
                 * 1: Clientes
                 * 2: Funcionarios
                 * 3: Transacoes
                 * 4: Relatorios
                 * 5: Menu
                */

                if(idScreen == 0)
                {

                    LoginScreen.Login(pathEmployeesFile);
                    idScreen = 5;

                } else if(idScreen == 1) 
                { 

                    actionScreen = ClientsScreen.Show();
                    switch(actionScreen)
                    {
                        case 1:
                            closeProgram = !ClientsScreen.SignUpNewClient(pathClientsFile);
                            break;
                        case 2:
                            closeProgram = !ClientsScreen.CheckExistingClientData(pathClientsFile);
                            break;
                        case 3:
                            closeProgram = !ClientsScreen.EditExistingClientAccount(pathClientsFile);
                            break;
                        case 4:
                            closeProgram = !ClientsScreen.TerminateExistingClientAccount(pathClientsFile);
                            break;
                        default:
                            break;
                    }

                    if (!closeProgram) idScreen = 5;

                } else if(idScreen == 2)
                {

                    actionScreen = EmployeesScreen.Show();
                    switch(actionScreen)
                    {
                        case 1:
                            closeProgram = !EmployeesScreen.SignUpNewEmployee(pathEmployeesFile);
                            break;
                        case 2:
                            closeProgram = !EmployeesScreen.EditExistingEmployeePassword(pathEmployeesFile);
                            break;
                        case 3:
                            closeProgram = !EmployeesScreen.TerminateExistingEmployeeAccount(pathEmployeesFile);
                            break;
                        default: 
                            break;
                    }

                    if (!closeProgram) idScreen = 5;

                } else if(idScreen == 3)
                {

                    if (TransactionsScreen.Show(pathClientsFile, pathTransactionsFolder)) idScreen = 5;
                    else closeProgram = true;

                } else if(idScreen == 4)
                {

                    actionScreen = ReportsScreen.Show();
                    switch(actionScreen) 
                    { 
                        case 1:
                            closeProgram = !ReportsScreen.ShowActiveClientsAndTheirBalances(pathClientsFile);
                            break;
                        case 2:
                            closeProgram = !ReportsScreen.ShowInactiveClients(pathClientsFile);
                            break;
                        case 3:
                            closeProgram = !ReportsScreen.ShowActiveEmployees(pathEmployeesFile);
                            break;
                        case 4:
                            closeProgram = !ReportsScreen.ShowUnsuccessfulTransactions(pathFailedTransactionsFolder);
                            break;
                        default:
                            break;
                    }

                    if (!closeProgram) idScreen = 5;

                } else if(idScreen == 5)
                {
                    int actionMenu;
                    do
                    {
                        actionMenu = Menu.Show();
                        switch (actionMenu)
                        {
                            case 0:
                            case 1:
                            case 2:
                            case 3:
                            case 4:
                                idScreen = actionMenu;
                                break;
                            case 5:
                                Messages.HelpMessage();
                                break;
                            default:
                                closeProgram = true;
                                break;
                        }
                    } while (actionMenu == 5);
                }
            }

            Messages.EndMessage();
        }
    }
}
