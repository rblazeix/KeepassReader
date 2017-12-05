using KeePassLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeePassW10.Services
{
    public class IconManager
    {
        private const string ICON_PATH_BASE = "ms-appx:///Assets/EntryIcons/";

        public static string GetIconPath(PwIcon iconId)
        {
            string path = ICON_PATH_BASE;

            switch (iconId)
            {
                case PwIcon.Key:
                    path += "C00_Password.png";
                    break;
                case PwIcon.World:
                    path += "C01_Package_Network.png";
                    break;
                case PwIcon.Warning:
                    path += "C02_MessageBox_Warning.png";
                    break;
                case PwIcon.NetworkServer:
                    path += "C03_Server.png";
                    break;
                case PwIcon.MarkedDirectory:
                    path += "C04_Klipper.png";
                    break;
                case PwIcon.UserCommunication:
                    path += "C05_Edu_Languages.png";
                    break;
                case PwIcon.Parts:
                    path += "C06_KCMDF.png";
                    break;
                case PwIcon.Notepad:
                    path += "C07_Kate.png";
                    break;
                case PwIcon.WorldSocket:
                    path += "C08_Socket.png";
                    break;
                case PwIcon.Identity:
                    path += "C09_Identity.png";
                    break;
                case PwIcon.PaperReady:
                    path += "C10_Kontact.png";
                    break;
                case PwIcon.Digicam:
                    path += "C11_Camera.png";
                    break;
                case PwIcon.IRCommunication:
                    path += "C12_IRKickFlash.png";
                    break;
                case PwIcon.MultiKeys:
                    path += "C13_KGPG_Key3.png";
                    break;
                case PwIcon.Energy:
                    path += "C14_Laptop_Power.png";
                    break;
                case PwIcon.Scanner:
                    path += "C15_Scanner.png";
                    break;
                case PwIcon.WorldStar:
                    path += "C16_Mozilla_Firebird.png";
                    break;
                case PwIcon.CDRom:
                    path += "C17_CDROM_Unmount.png";
                    break;
                case PwIcon.Monitor:
                    path += "C18_Display.png";
                    break;
                case PwIcon.EMail:
                    path += "C19_Mail_Generic.png";
                    break;
                case PwIcon.Configuration:
                    path += "C20_Misc.png";
                    break;
                case PwIcon.ClipboardReady:
                    path += "C21_KOrganizer.png";
                    break;
                case PwIcon.PaperNew:
                    path += "C22_ASCII.png";
                    break;
                case PwIcon.Screen:
                    path += "C23_Icons.png";
                    break;
                case PwIcon.EnergyCareful:
                    path += "C24_Connect_Established.png";
                    break;
                case PwIcon.EMailBox:
                    path += "C25_Folder_Mail.png";
                    break;
                case PwIcon.Disk:
                    path += "C26_FileSave.png";
                    break;
                case PwIcon.Drive:
                    path += "C27_NFS_Unmount.png";
                    break;
                case PwIcon.PaperQ:
                    path += "C28_Message.png";
                    break;
                case PwIcon.TerminalEncrypted:
                    path += "C29_KGPG_Term.png";
                    break;
                case PwIcon.Console:
                    path += "C30_Konsole.png";
                    break;
                case PwIcon.Printer:
                    path += "C31_FilePrint.png";
                    break;
                case PwIcon.ProgramIcons:
                    path += "C32_FSView.png";
                    break;
                case PwIcon.Run:
                    path += "C33_Run.png";
                    break;
                case PwIcon.Settings:
                    path += "C34_Configure.png";
                    break;
                case PwIcon.WorldComputer:
                    path += "C35_KRFB.png";
                    break;
                case PwIcon.Archive:
                    path += "C36_Ark.png";
                    break;
                case PwIcon.Homebanking:
                    path += "C37_KPercentage.png";
                    break;
                case PwIcon.DriveWindows:
                    path += "C38_Samba_Unmount.png";
                    break;
                case PwIcon.Clock:
                    path += "C39_History.png";
                    break;
                case PwIcon.EMailSearch:
                    path += "C40_Mail_Find.png";
                    break;
                case PwIcon.PaperFlag:
                    path += "C41_VectorGfx.png";
                    break;
                case PwIcon.Memory:
                    path += "C42_KCMMemory.png";
                    break;
                case PwIcon.TrashBin:
                    path += "C43_Trashcan_Full.png";
                    break;
                case PwIcon.Note:
                    path += "C44_KNotes.png";
                    break;
                case PwIcon.Expired:
                    path += "C45_Cancel.png";
                    break;
                case PwIcon.Info:
                    path += "C46_Help.png";
                    break;
                case PwIcon.Package:
                    path += "C47_KPackage.png";
                    break;
                case PwIcon.Folder:
                    path += "C48_Folder.png";
                    break;
                case PwIcon.FolderOpen:
                    path += "C49_Folder_Blue_Open.png";
                    break;
                case PwIcon.FolderPackage:
                    path += "C50_Folder_Tar.png";
                    break;
                case PwIcon.LockOpen:
                    path += "C51_Decrypted.png";
                    break;
                case PwIcon.PaperLocked:
                    path += "C52_Encrypted.png";
                    break;
                case PwIcon.Checked:
                    path += "C53_Apply.png";
                    break;
                case PwIcon.Pen:
                    path += "C54_Signature.png";
                    break;
                case PwIcon.Thumbnail:
                    path += "C55_Thumbnail.png";
                    break;
                case PwIcon.Book:
                    path += "C56_KAddressBook.png";
                    break;
                case PwIcon.List:
                    path += "C57_View_Text.png";
                    break;
                case PwIcon.UserKey:
                    path += "C58_KGPG.png";
                    break;
                case PwIcon.Tool:
                    path += "C59_Package_Development.png";
                    break;
                case PwIcon.Home:
                    path += "C60_KFM_Home.png";
                    break;
                case PwIcon.Star:
                    path += "C61_Services.png";
                    break;
                case PwIcon.Tux:
                    path += "C62_Tux.png";
                    break;
                case PwIcon.Feather:
                    path += "C63_Feather.png";
                    break;
                case PwIcon.Apple:
                    path += "C64_Apple.png";
                    break;
                case PwIcon.Wiki:
                    path += "C65_W.png";
                    break;
                case PwIcon.Money:
                    path += "C66_Money.png";
                    break;
                case PwIcon.Certificate:
                    path += "C67_Certificate.png";
                    break;
                case PwIcon.BlackBerry:
                    path += "C68_Smartphone.png";
                    break;
                default:
                    path = String.Empty;
                    break;
            }

            return path;
        }
    }
}
