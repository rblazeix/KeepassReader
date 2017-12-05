using KeePassW10.Common;
using KeePassW10.ViewModels;
using KeePassW10.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.Security.Cryptography;
using Windows.Storage;

namespace KeePassW10.Services
{
    /// <summary>
    /// Encapsulate the retrieval of the password database.
    /// It can be through a prompt, or the system password vault, secured using Windows Hello
    /// </summary>
    public class DatabasePasswordProvider
    {
        private const string WINDOWS_HELLO_SERVICE_NAME = "KeePassReader";
        private const string PASSWORD_VAULT_USER_NAME = "DefaultUser";

        /// <summary>
        /// Retrieve the password to use to open the given database
        /// </summary>
        /// <param name="databaseName">name of the database</param>
        /// <returns>credentials to use to open this database</returns>
        public async static Task<PasswordParameter> GetDatabasePasswordAsync(string databaseName)
        {
            PasswordParameter credentials = null;

            // Try to use Windows Hello
            if (SettingsViewModel.Instance.WindowsHelloEnabled &&
                await KeyCredentialManager.IsSupportedAsync()
                )
            {
                // Try to open an already generated Windows Hello credentials
                var passportCredentials = await KeyCredentialManager.OpenAsync(WINDOWS_HELLO_SERVICE_NAME);

                if (passportCredentials.Status == KeyCredentialStatus.Success)
                {
                    var signRes = await passportCredentials.Credential.RequestSignAsync(
                        CryptographicBuffer.ConvertStringToBinary("LoginAuth", BinaryStringEncoding.Utf8));

                    if (signRes.Status == KeyCredentialStatus.Success)
                    {
                        // User confirmed its identity, retrieve the credentials from the system Password Vault
                        try
                        {
                            credentials = await LoadDatabasePasswordAsync(databaseName);
                        }
                        catch (Exception)
                        {
                            // Prompt for credentials, then store them
                            PasswordParameter password = await ShowPasswordDialogAsync(databaseName);
                            StoreDatabasePassword(databaseName, password);
                            credentials = password;
                        }
                    }
                    else if (signRes.Status == KeyCredentialStatus.UserCanceled )
                    {
                        // User cancelled the credential
                        throw new TaskCanceledException();
                    }
                }
                else
                {
                    // No credentials available, need to create it
                    var passportCredentialCreation = await KeyCredentialManager.RequestCreateAsync(WINDOWS_HELLO_SERVICE_NAME, KeyCredentialCreationOption.FailIfExists);

                    if (passportCredentialCreation.Status == KeyCredentialStatus.Success ||
                        passportCredentialCreation.Status == KeyCredentialStatus.CredentialAlreadyExists)
                    {
                        // Prompt for credentials, then store them
                        PasswordParameter password = await ShowPasswordDialogAsync(databaseName);
                        StoreDatabasePassword(databaseName, password);
                        credentials = password;
                    }
                    else if (passportCredentialCreation.Status == KeyCredentialStatus.UserCanceled)
                    {
                        // User cancelled the credential
                        throw new TaskCanceledException();
                    }
                }


            }

            if (credentials == null )
            {
                // No Windows Hello, use standard password
                credentials = await ShowPasswordDialogAsync(databaseName);
            }
            return credentials;
        }

        /// <summary>
        /// Clears PasswordVault of any saved password
        /// </summary>
        public static void ClearAllSavedPasswords()
        {
            try
            {
                var vault = new Windows.Security.Credentials.PasswordVault();
                foreach (var p in vault.RetrieveAll())
                {
                    vault.Remove(p);
                }
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Clears any saved password for the given database
        /// </summary>
        /// <param name="databaseName"></param>
        public static void ClearSavedPasswordForDatabase(string databaseName)
        {
            // No need to try to remove any saved password if WIndows Hello is not enabled
            if (SettingsViewModel.Instance.WindowsHelloEnabled)
            {
                try
                {
                    var vault = new Windows.Security.Credentials.PasswordVault();
                    foreach (var p in vault.FindAllByResource(databaseName))
                    {
                        vault.Remove(p);
                    }
                }
                catch (Exception) { }
            }
        }


        private async static Task<PasswordParameter> ShowPasswordDialogAsync(string databaseName)
        {
            // Builds a custom dialog with a text field
            PasswordDialog pwdDialog = new PasswordDialog(databaseName);

            // The result is
            PasswordParameter pwd = new PasswordParameter();

            await pwdDialog.ShowAsync();

            if (pwdDialog.Result)
            {
                if (pwdDialog.ViewModel.UsePassword)
                {
                    pwd.Password = pwdDialog.ViewModel.Password;
                }

                if (pwdDialog.ViewModel.UseKeyFile && pwdDialog.ViewModel.KeyFile != null)
                {
                    pwd.KeyFile = pwdDialog.ViewModel.KeyFile;
                }
                // User validated the password
                return pwd;
            }
            else
            {
                throw new TaskCanceledException();
            }
        }

        /// <summary>
        /// Save the given credentials in the os password vault
        /// </summary>
        /// <param name="password"></param>
        private static void StoreDatabasePassword( string databaseName, PasswordParameter password)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();
            vault.Add(
                new PasswordCredential(databaseName, PASSWORD_VAULT_USER_NAME, password.Password));

        }

        /// <summary>
        /// Load previously stored credentials from the password vault, if any
        /// </summary>
        /// <param name="databaseName"></param>
        /// <returns></returns>
        private async static Task<PasswordParameter> LoadDatabasePasswordAsync(string databaseName)
        {
            var vault = new Windows.Security.Credentials.PasswordVault();
            PasswordCredential cred = vault.Retrieve(databaseName, PASSWORD_VAULT_USER_NAME);

            // Also try to retrieve the KeyFile, if any was saved
            StorageFile keyFile = await PasswordDialogViewModel.GetStoredKeyFileAsync(databaseName);

            return new PasswordParameter() {
                Password = cred.Password,
                KeyFile = keyFile

            };
        }
    }
}
