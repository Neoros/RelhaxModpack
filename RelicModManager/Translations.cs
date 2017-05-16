?using System.Collections;

namespace RelhaxModpack
{
    //A static class to exist throughout the entire application life, will always have translations
    public static class Translations
    {
        //Enumerator to determine which translated string to return
        public enum Languages { English = 0, German = 1, Polish = 2 };
        public static Languages language = Languages.English;//set it to this default
        public static Hashtable english = new Hashtable();
        public static Hashtable german = new Hashtable();
        public static Hashtable polish = new Hashtable();
        //load hashes on application startup
        //Translations.loadHashes();

        public static string getTranslatedString(string componetName)
        {
            if (language == Languages.English)
            {
                if (english.Contains(componetName))
                {
                    return (string)english[componetName];
                }
            }
            else 
			{
				if (language == Languages.German)
            		{
                		if (german.Contains(componetName))
                		{
                    		return (string)german[componetName];
                		}
				else if (language == Languages.Polish)
            		{
                		if (polish.Contains(componetName))
                		{
                    		return (string)polish[componetName];
                		}

            	}
            Settings.appendToLog("ERROR: no value in language hash for key: " + componetName + ": Language: " + language);
            return componetName;
        }
        //method to load each translated string based on which language is selected
        public static void loadHashes()
        {
            //Syntax is as follows:
            //languageName.Add("componetName","TranslatedString");
            //Section: MainWindow
            //Componet: installRelhaxMod
            //The button for installing the modpack
            english.Add("installRelhaxMod", "Start Mod Selection");
            german.Add("installRelhaxMod", "Auswahl der Mods");
            Polish.Add("installRelhaxMod", "Przejdź Do Wyboru Modyfikacji");

            //Componet: uninstallRelhaxMod
            //
            english.Add("uninstallRelhaxMod", "Uninstall Relhax Modpack");
            german.Add("uninstallRelhaxMod", "Relhax Modpack deinstallieren");
            Polish.Add("uninstallRelhaxMod", "Odinstaluj Paczkę Relhax");

            //Componet: forceManuel
            //
            english.Add("forceManuel", "Force manual game detection");
            german.Add("forceManuel", "Erzwinge manuelle Spielerkennung");
            Polish.Add("forceManuel", "Wymuś ręczną weryfikację ścieżki gry");

            //Componet: forceManuel
            //
            english.Add("languageSelectionGB", "Language Selection");
            german.Add("languageSelectionGB", "Sprachauswahl");
            Polish.Add("languageSelectionGB", "Wybór języka");

            //Componet: formPageLink
            //
            english.Add("formPageLink", "View Modpack Form Page");
            german.Add("formPageLink", "Zeige Modpack Formularseite");
            Polish.Add("formPageLink", "Forum Paczki (NA)");

            //Componet: saveUserDataCB
            //
            english.Add("saveUserDataCB", "Save user data");
            german.Add("saveUserDataCB", "Benutzerdaten speichern");
            Polish.Add("saveUserDataCB", "Zapisz ustawienia użytkownika");

            //Componet: cleanInstallCB
            //
            english.Add("cleanInstallCB", "Clean Installation (Recommended)");
            german.Add("cleanInstallCB", "Saubere Installation (Empfohlen)");
            Polish.Add("cleanInstallCB", "Czysta instalacja (Zalecane)");

            //Componet: cancerFontCB
            //
            english.Add("cancerFontCB", "Comic Sans Font");
            german.Add("cancerFontCB", "Comic Sans Schriftart");
            Polish.Add("cancerFontCB", "Czcionka Comic Sans");

            //Componet: backupModsCheckBox
            //
            english.Add("backupModsCheckBox", "Backup current mods folder");
            german.Add("backupModsCheckBox", "Sicherung des aktuellen Modsordner");
            Polish.Add("backupModsCheckBox", "Zrób kopię zapasową obecnego pliku z modyfikacjami");

            //Componet: settingsGroupBox
            //
            english.Add("settingsGroupBox", "RelHax ModPack Settings");
            german.Add("settingsGroupBox", "RelHax ModPack Einstellungen");
            Polish.Add("settingsGroupBox", "Ustawienia Paczki RelHax");

            //Componet: darkUICB
            //
            english.Add("darkUICB", "Dark UI");
            german.Add("darkUICB", "Dunkle Benutzeroberflaeche");
            Polish.Add("darkUICB", "Ciemny Interfejs");

            //Componet: cleanUninstallCB
            //
            english.Add("cleanUninstallCB", "Clean uninstallation");
            german.Add("cleanUninstallCB", "Saubere Deinstallation");
            Polish.Add("cleanUninstallCB", "Czysta deinstalacja");

            //Componet: saveLastInstallCB
            //
            english.Add("saveLastInstallCB", "Save last install\'s config");
            german.Add("saveLastInstallCB", "Speicherung der letzten Installation");
            Polish.Add("saveLastInstallCB", "Zapisz ostatnią konfigurację instalacji");

            //Componet: largerFontButton
            //
            english.Add("largerFontButton", "Larger Font");
            german.Add("largerFontButton", "Groessere Schriftart");
            Polish.Add("largerFontButton", "Większa czcionka");

            //Componet: loadingImageGroupBox
            //
            english.Add("loadingImageGroupBox", "Loading Image");
            german.Add("loadingImageGroupBox", "Bild laden");
            Polish.Add("loadingImageGroupBox", "Ładowanie obrazka");

            //Componet: standardImageRB
            //
            english.Add("standardImageRB", "Standard");
            german.Add("standardImageRB", "Standard");
            Polish.Add("standardImageRB", "Podstawowe");

            //Componet: findBugAddModLabel
            //
            english.Add("findBugAddModLabel", "Find a bug? Want a mod added?");
            german.Add("findBugAddModLabel", "Fehler gefunden? Willst Du einen Mod hinzufuegen");
            Polish.Add("findBugAddModLabel", "Znalazłeś błąd? Chcesz dodać mod?");

            //Componet: cancelDownloadButton
            //
            english.Add("cancelDownloadButton", "Cancel Download");
            german.Add("cancelDownloadButton", "Download abbrechen");
            Polish.Add("cancelDownloadButton", "Anuluj ściąganie");

            //Section: FirstLoadHelper
            //Componet: helperText
            //
            english.Add("helperText", "Welcome to the RelHax Modpack! I have tried to make the modpack as straight-forward as possible, but questions may still arise. Hover over (or right click) a setting to have it explained. You won't see this dialog box again, unless you delete the settings xml file.");
            german.Add("helperText", "Willkommen im RelHax Modpack! Ich habe versucht, das Modpack so einfach wie möglich zu gestalten, aber Fragen können dennoch entstehen. Rechtsklick auf eine Einstellung erklaert diese dann. Du siehst diese Dialogbox nicht mehr, ausser du loescht die xml Datei.");
            Polish.Add("helperText", "Witamy w paczce RelHax! Próbowałem stworzyć jak najprostszą w użytku paczkę modyfikacji, ale wciąż możesz mieć pytania. Kliknik PPM na opcji, by wyświetlić opis. Nie zobaczysz tej wiadomości ponownie, dopóki nie usuniesz pliku ustawień xml.");
            
            //Section: ModSelectionList
            //Componet: continueButton
            //
            english.Add("continueButton", "Install");
            german.Add("continueButton", "Installieren");
            Polish.Add("continueButton", "Zainstaluj");

            //Componet: cancelButton
            //
            english.Add("cancelButton", "Cancel");
            german.Add("cancelButton", "Abbrechen");
            Polish.Add("cancelButton", "Anuluj");

            //Componet: helpLabel
            //
            english.Add("helpLabel", "right-click a mod name to preview it");
            german.Add("helpLabel", "Klick mit rechten Maustaste auf einen Mod-Namen, um eine Vorschau zu sehen");
            Polish.Add("helpLabel", "PPM by wyświetlić opis");

            //Componet: loadConfigButton
            //
            english.Add("loadConfigButton", "Load selection");
            german.Add("loadConfigButton", "Auswahl laden");
            Polish.Add("loadConfigButton", "Wczytaj konfigurację z pliku");

            //Componet: saveConfigButton
            //
            english.Add("saveConfigButton", "Save selection");
            german.Add("saveConfigButton", "Auswahl speichern");
            Polish.Add("saveConfigButton", "Zapisz konfigurację w pliku");

            //Componet: label2
            //
            english.Add("label2", "\"*\" ");
            german.Add("label2", "Ein Tab mit einem\"*\" kann nur eins der Mods ausgewählt werden.");
            Polish.Add("label2", "\*\ wskazuje pojedynczą kartę wyboru");

            //Componet: clearSelectionsButton
            //
            english.Add("clearSelectionsButton", "Clear selections");
            german.Add("clearSelectionsButton", "Auswahl loeschen");
            Polish.Add("clearSelectionsButton", "Wyczyść wybór");

            //Componet: readingDatabase
            //
            english.Add("readingDatabase", "Reading Database");
            german.Add("readingDatabase", "Lese Datenbank");
            Polish.Add("readingDatabase", "Wczytywanie baz danych");

            //Componet: buildingUI
            //
            english.Add("buildingUI", "Building UI");
            german.Add("buildingUI", "Erstelle UI");
            Polish.Add("buildingUI", "Budowanie interfejsu");

            //Section: Preview
            //Componet: nextPicButton
            //
            english.Add("nextPicButton", "next");
            german.Add("nextPicButton", "naechstes");
            Polish.Add("nextPicButton", "Dalej");

            //Componet: previousPicButton
            //
            english.Add("previousPicButton", "previous");
            german.Add("previousPicButton", "vorheriges");
            Polish.Add("previousPicButton", "Wstecz");

            //Componet: devLinkLabel
            //
            english.Add("devLinkLabel", "Developer Website");
            german.Add("devLinkLabel", "Entwickler Webseite");
            Polish.Add("devLinkLabel", "Strona Dewelopera");

            //Section: VersionInfo
            //Componet: updateAcceptButton
            //
            english.Add("updateAcceptButton", "yes");
            german.Add("updateAcceptButton", "ja");
            Polish.Add("updateAcceptButton", "Tak");

            //Componet: updateDeclineButton
            //
            english.Add("updateDeclineButton", "no");
            german.Add("updateDeclineButton", "nein");
            Polish.Add("updateDeclineButton", "Nie");

            //Componet: newVersionAvailableLabel
            //
            english.Add("newVersionAvailableLabel", "New Version Available");
            german.Add("newVersionAvailableLabel", "Neue Version verfuegbar");
            Polish.Add("newVersionAvailableLabel", "Dostępna Nowa Wersja");

            //Componet: updateQuestionLabel
            //
            english.Add("updateQuestionLabel", "Update?");
            german.Add("updateQuestionLabel", "Aktualisieren?");
            Polish.Add("updateQuestionLabel", "Zaktualizować?");

            //Componet: problemsUpdatingLabel
            //
            english.Add("problemsUpdatingLabel", "If you are having problems updating, please");
            german.Add("problemsUpdatingLabel", "Wenn Sie Probleme mit der Aktualisierung haben, bitte");
            Polish.Add("problemsUpdatingLabel", "Jeśli masz problemy z aktualizają proszę");

            //Componet: 
            //
            english.Add("clickHereUpdateLabel", "click here.");
            german.Add("clickHereUpdateLabel", "klick hier.");
            Polish.Add("clickHereUpdateLabel", "kliknij tutaj");

            //Section: PleaseWait
            //Componet: label1
            //
            english.Add("label1", "Loading...please wait...");
            german.Add("label1", "Laedt...bitte warten...");
            Polish.Add("label1", "Ładowanie... proszę czekać...");

            //Section: Messages of MainWindow
            //Componet: 
            //
            english.Add("Downloading", "Downloading");
            german.Add("Downloading", "Wird heruntergeladen");
            Polish.Add("Downloading", "Pobieranie");

            //Componet: 
            //
            english.Add("patching", "patching");
            german.Add("patching", "patching");
            Polish.Add("patching", "patchowanie");

            //Componet: 
            //
            english.Add("done", "done");
            german.Add("done", "fertig");
            Polish.Add("done", "zrobione");

            //Componet: 
            //
            english.Add("idle", "idle");
            german.Add("idle", "Leerlauf");
            Polish.Add("idle", "bezczynny");

            //Componet: 
            //
            english.Add("status", "Status:");
            german.Add("status", "Status:");
            Polish.Add("status", "Stan");

            //Componet: 
            //
            english.Add("canceled", "canceled");
            german.Add("canceled", "abgebrochen");
            Polish.Add("canceled", "anulowano");

            //Componet: 
            //
            english.Add("appSingleInstance", "Checking for single instance");
            german.Add("appSingleInstance", "Prüfung auf Einzelinstanz");
            Polish.Add("appSingleInstance", "Sprawdzanie ");
            
            //Componet: 
            //
            english.Add("checkForUpdates", "Checking for updates");
            german.Add("checkForUpdates", "Auf Updates prüfen");
             Polish.Add("checkForUpdates", "Sprawdzanie aktualizacji");

            //Componet: 
            //
            english.Add("verDirStructure", "Verifying directory structure");
            german.Add("verDirStructure", "Verifizierung der Verzeichnisstruktur");
            Polish.Add("verDirStructure", "Sprawdzanie struktury dostępu");

            //Componet: 
            //
            english.Add("loadingSettings", "Loading Settings");
            german.Add("loadingSettings", "Einstellungen laden");
            Polish.Add("loadingSettings", "Ładowanie ustawnień"); 

            //Componet: 
            //
            english.Add("loadingTranslations", "Loading Translations");
            german.Add("loadingTranslations", "Laden der Uebersetzungen");
            Polish.Add("loadingTranslations", "ładowanie tłumaczenia"); 

            //Componet: 
            //
            english.Add("loading", "Loading");
            german.Add("loading", "Laden");
            Polish.Add("loading", "Ładowanie"); 

            //Componet: 
            //
            english.Add("uninstalling", "Uninstalling");
            german.Add("uninstalling", "Deinstallieren");
            Polish.Add("uninstalling", "Deinstalacja w toku"); 

            //Componet: 
            //
            english.Add("installingFonts", "Installing Fonts");
            german.Add("installingFonts", "Installieren von Schriftarten");
            Polish.Add("installingFonts", "Instalowanie czcionek"); 

            //Componet: 
            //
            english.Add("loadingExtractionText", "Loading Extraction Text");
            german.Add("loadingExtractionText", "Extraktionstext laden");
            Polish.Add("loadingExtractionText", "Ładowanie tekstu"); 

            //Componet: 
            //
            english.Add("extractingRelhaxMods", "Extracting RelHax Mods");
            german.Add("extractingRelhaxMods", "Extrahieren von RelHax Mods");
            Polish.Add("extractingRelhaxMods", "Wypakowywanie modyfikacji RelHax");

            //Componet: 
            //
            english.Add("extractingUserMods", "Extracting User Mods");
            german.Add("extractingUserMods", "Extrahieren von benutzerdefinierten Mods");
            Polish.Add("extractingUserMods", "Wypakowywanie modyfikacji użytkownika");

            //Componet: 
            //
            english.Add("startingSmartUninstall", "Starting smart uninstall");
            german.Add("startingSmartUninstall", "Starten der intelligenten Deinstallation");
            Polish.Add("startingSmartUninstall", "Rozpoczynanie inteligentnej deinstalacji");

            //Componet: 
            //
            english.Add("copyingFile", "Copying file");
            german.Add("copyingFile", "Kopieren von Dateien");
            polish.Add("copyingFile", "Kopiowanie plików");

            //Componet: 
            //
            english.Add("deletingFile", "Deleting file");
            german.Add("deletingFile", "Loeschen von Dateien");
            polish.Add("deletingFile", "Usuwanie plików");

            //Componet: 
            //
            english.Add("of", "of");
            german.Add("of", "von");
            polish.Add("of", "z");

            //Componet: 
            //
            english.Add("forceManuelDescription", "This option is for forcing a manual World of Tanks game" +
                    "location detection. Check this if you are having problems with automatically locating the game.");
            german.Add("forceManuelDescription", "Diese Option ist für die Erzwingung einer manuellen World of Tanks Spiel-" +
                    "Speicherort-Erkennung. Überpruefe dies, wenn Du Probleme mit der automatischen Suche des Spiels hast.");
            polish.Add("forceManuelDescription", "Ta opcja wymusza ręczne znalezienie lokacji gry World of Tanks." +
                    "Zaznacz, jeśli masz problem z automatycznym znalezieniem ścieżki dostępu do gry.");
            
            //Componet: 
            //
            english.Add("cleanInstallDescription", "This recommended option will empty your res_mods folder before installing" +
                    "your new mod selections. Unless you know what you are doing, it is recommended that you keep this on to avoid problems.");
            german.Add("cleanInstallDescription", "Diese empfohlene Option leert den Ordner res_mods vor der Installation" +
                    "Deiner neuen Mod-Auswahl. Bis du genau weißt, was du tust, empfohlen wir, dass du das weiter behältst, um Probleme zu vermeiden.");
            polish.Add("cleanInstallDescription", "To rekomendowane ustawienie usunie zawartość folderu res_mods przed instalacją" +
                     "nowej konfiguracji modów. Jeśli nie wiesz co robisz zalecamy włączyć tą opcję, aby uniknąć problemów.";
            
            //Componet: 
            //
            english.Add("backupModsDescription", "Select this to make a backup of your current res_mods folder." +
                    "They are stored in the 'RelHaxModBackup' folder, saved in a folder inside by a time stamp.");
            german.Add("backupModsDescription", "Waehle diese Option, um eine Sicherungskopie Deines aktuellen res_mods-Ordners zu erstellen." +
                    "Sie werden im Ordner 'RelHaxModBackup' gespeichert und in einem Ordner nach innen durch einen Zeitstempel gespeichert.");
            polish.Add("backupModsDescription", "Zaznacz, aby zrobić kopię zapasową folderu res_mods." +
                     "Pliki będą przechowane w folderze RelHaxModBackup, zapisane w folderze oznaczonym datą.");
                       
            //Componet: 
            //
            english.Add("comicSansDescription", "Enable Comic Sans font");
            german.Add("comicSansDescription", "Schriftart Comic Sans aktivieren");
            polish.Add("comicSansDescription", "Włącz czcionkę Comic Sans");

            //Componet: 
            //
            english.Add("enlargeFontDescription", "Enlarge font");
            german.Add("enlargeFontDescription", "Schriftart vergroessern");
            polish.Add("enlargeFontDescription", "Powiększ czcionkę");

            //Componet: 
            //
            english.Add("selectGifDesc", "Select a loading gif for the mod preview window.");
            german.Add("selectGifDesc", "Waehle ein Lade-Gif fuer das Vorschaufenster des Mods.");
            polish.Add("selectGifDesc", "Załaduj gif w oknie podglądu.");

            //Componet: 
            //
            english.Add("saveLastConfigInstall", "If this is selected, the installer will, upon selection window showing, load the last installed config you used.");
            german.Add("saveLastConfigInstall", "Wenn dies ausgewählt ist, lädt das Installationsprogramm die zuletzt installierte Config im Auswahlfenster, die Du verwendet hast.");
			polish.Add("saveLastConfigInstall", "Przy zaznaczeniu, instalator załaduje ostatnią użytą konfigurację w oknie wyboru modyfikacji.");
					   
            //Componet:
            //
            english.Add("saveUserDataDesc", "If this is selected, the installer will save user created data (like session stats from previous battles)");
            german.Add("saveUserDataDesc", "Wenn dies ausgewählt ist, speichert das Installationsprogramm die vom Benutzer erstellten Daten (wie Sitzungsstatistiken aus früheren Gefechten)");
			polish.Add("saveUserDataDesc", "Przy zaznaczeniu, instalator zachowa pliki danych użytkownika (takie jak statystyki sesji z poprzednich bitew)");
			
            //Componet: 
            //
            english.Add("cleanUninstallDescription", "Selected - All mods will be erased\nNot Selected - Only Modpack installed mods will be erased");
            german.Add("cleanUninstallDescription", "Ausgewählt - Alle Mods werden gelöscht\nNicht ausgewählt - Nur Mods, die vom Modpack installiert wurden, werden gelöscht");
			polish.Add("cleanUninstallDescription", "Zaznaczone - Wszystkie mody zostaną usunięte\nNie zaznaczone - Tylko zainstalowane mody z paczki zostaną usunięte");
					   
            //Componet: 
            //
            english.Add("darkUIDesc", "Toggle the DarkUI mode. Usefull for working with the modpack at night.");
            german.Add("darkUIDesc", "Auf dunklen UI Modus umschalten. Nützlich für die Arbeit mit dem Modpack in der Nacht.");
			polish.Add("darkUIDesc", "Zmień interfejs na ciemny. Przydatneprzy pracy z paczką w nocy.");
					   
            //Componet: 
            //
            english.Add("failedToDownload_1", "Failed to download ");
            german.Add("failedToDownload_1", "Fehler beim Herunterladen ");
			polish.Add("failedToDownload_1", "Ściąganie zakończone niepowodzeniem, plik ");

            //Componet: 
            //
            english.Add("failedToDownload_2", ". If you know which mod this is, uncheck it and you should be fine. It will be fixed soon. Restart this when it exits");
            german.Add("failedToDownload_2", ". Wenn du weisst, welcher Mod das ist, deaktiviere ihn und alles sollte funktionieren. Es wird bald behoben. Starte neu, wenn es besteht");
			polish.Add("failedToDownload_2", ". Jeśli wiesz który to mod, odznacz go i wszystko powinno byćw porządku. Wkrótce naprawimy błąd. Zrestartuj, jeśli problem pojawia się ponownie.");
					   
            //Componet: 
            //
            english.Add("fontsPromptInstallHeader", "Admin to install fonts?");
            german.Add("fontsPromptInstallHeader", "Admin zum Installieren von Schriftarten?");
			polish.Add("fontsPromptInstallHeader", "Uprawnienia administratora, by zainstalować czcionki?");
					   
            //Componet: 
            //
            english.Add("fontsPromptInstallText", "Do you have admin rights?");
            german.Add("fontsPromptInstallText", "Hast Du Admin-Rechte?");
			polish.Add("fontsPromptInstallText", "Czy masz uprawnienia administratora?");
					   
            //Componet: 
            //
            english.Add("fontsPromptError_1", "Unable to install fonts. Some mods may not work properly. Fonts are located in ");
            german.Add("fontsPromptError_1", "Schriftarten können nicht installiert werden. Einige Mods funktionieren möglicherweise nicht richtig. Schriften befinden sich in ");
			polish. Add("fontsPromptError_1", "Niepowodzenie przy instalacji czcionek. Niektóre modyfikacje mogą nie działać prawidłowo. Czcionki znajdują się w ");
						
            //Componet: 
            //
            english.Add("fontsPromptError_2", "\\_fonts. Either install them yourself or run this again as Administrator.");
            german.Add("fontsPromptError_2", "\\_fonts. Entweder installiere sie selbst oder führe es erneut als Administrator aus.");
			polish.Add("fontsPromptError_2", "\\_fonts. Albo zainstalujesz je własnoręcznie, albo uruchom jako administrator.");
					   
            //Componet: 
            //
            english.Add("cantDownloadNewVersion", "Unable to download new version, exiting.");
            german.Add("cantDownloadNewVersion", "Die neue Version kann nicht heruntergeladen werden.");
			polish.Add("cantDownloadNewVersion", "Niepowodzenie przy pobieraniu nowej wersji.");

            //Componet: 
            //
            english.Add("cantStartNewApp", "Unable to start application, but it is located in \n");
            german.Add("cantStartNewApp", "Kann die Anwendung nicht starten, aber sie befindet sich in \n");
			polish.Add("cantStartNewApp", "Niepowodzenie przy uruchamianiu aplikacji znajdującej się w \n");
					   
            //Componet: 
            //
            english.Add("autoDetectFailed", "The auto-detection failed. Please use the 'force manual' option");
            german.Add("autoDetectFailed", "Die automatische Erkennung ist fehlgeschlagen. Bitte benutzen Sie die 'erzwinge manuelle' Option");
			polish.Add("autoDetectFailed", "Niepowodzenie automatycznego wykrywania. Proszę wybrać opcję ręcznego znajdowania ścieżki gry.");
					   
            //Componet: 
            //
            english.Add("anotherInstanceRunning", "CRITICAL: Another Instance of the relic mod manager is already running");
            german.Add("anotherInstanceRunning", "KRITISCH: Eine weitere Instanz des Relic Mod Managers läuft bereits");
			polish.Add("anotherInstanceRunning", "BŁĄD KRYTYCZNY: Inna instancja relic mod managera jest uruchomiona");
					   
            //Componet: 
            //
            english.Add("skipUpdateWarning", "WARNING: You are skipping updating. Database Compatability is not guarenteed");
            german.Add("skipUpdateWarning", "WARNUNG: Sie überspringen die Aktualisierung. Datenbankkompatibilität ist nicht garantiert");
			polish.Add("skipUpdateWarning", "OSTRZEŻENIE: Pomijasz aktualizację! Może wystąpić niezgodność wersji.");
					   
            //Componet: 
            //
            english.Add("patchDayMessage", "The modpack is curretly down for patch day testing and mods updating. Sorry for the inconvience. If you are a database manager, please add the command arguement");
            german.Add("patchDayMessage", "Das Modpack ist zur Zeit für Patch-Tag-Tests und das Updaten von Mods nicht erreichbar. Entschuldige die Unannehmlichkeiten. Wenn Sie ein Datenbankmanager sind, fügen Sie bitte das Befehlsargument hinzu");
			polish.Add("patchDayMessage", "Paczka nie działa ze względu na testy i aktualizację modyfikacji. Przepraszamy za utrudnienia. Jeśli zarządzasz bazą danych, proszę dodać odpowiednią komendę");
					   
            //Componet: 
            //
            english.Add("configNotExist", " does NOT exist, loading in regular mode");
            german.Add("configNotExist", " existiert nicht, laedt im regulaeren Modus");
            polish.Add("configNotExist", " nie istnieje, ładowanie podstawowego trybu");
					   
            //Componet: 
            //
            english.Add("autoAndClean", "ERROR: clean installation is set to false. You must set this to true and restart the application for auto install to work. Loading in regular mode.");
            german.Add("autoAndClean", "FEHLER: Die saubere Installation ist auf false eingestellt. Du musst dies auf true setzen und die Anwendung für die automatische Installation neu starten, damit sie funktioniert. Laedt im regulaeren Modus.");
            polish.Add("autoAndClean", "BŁĄD: wyłączono czystą instalację. Musisz ją włączyć i ponownie uruchomić aplikację, by automatyczna instalacja zadziałała. Ładowanie w trybie podstawowym.");
					   
            //Componet: 
            //
            english.Add("autoAndFirst", "ERROR: First time loading cannot be an auto install mode, loading in regular mode");
            german.Add("autoAndFirst", "FEHLER: Erstmaliges Laden kann kein automatischer Installationsmodus sein, lade im regulaeren Modus");
			polish.Add("autoAndFirst", "BŁĄD: Pierwsze ładowanie nie może być automatyczną instalacją, ładowanie w trybie podstawowym");
					   
            //Componet: 
            //
            english.Add("confirmUninstallHeader", "Confirmation");
            german.Add("confirmUninstallHeader", "Bestätigung");
			polish.Add("confirmUninstallHeader", "Potwierdź");

            //Componet: 
            //
            english.Add("confirmUninstallMessage", "Confirm you wish to uninstall?");
            german.Add("confirmUninstallMessage", "Bestätige, wenn du installieren möchtest?");
			polish.Add("confirmUninstallMessage", "Potwierdzić deinstalację?");

            //Componet: 
            //
            english.Add("uninstallingText", "Uninstalling...");
            german.Add("uninstallingText", "Deinstalliere...");
			polish.Add("uninstallingText", "Deinstalacja w toku...");		   

            //Componet: 
            //
            english.Add("specialMessage1", "If you are seeing this, it means that you have a specific computer configuration that is affected by a bug I can't replicate on my developer system. It's harmless, but if you could send your relHaxLog to me I can fix it and you can stop seeing this message");
            german.Add("specialMessage1", "Wenn Du dies siehst, bedeutet dies, dass Du eine bestimmte Computer-Konfiguration hast, die von einem Fehler betroffen ist, den ich nicht auf meinem Entwicklersystem replizieren kann. Es ist harmlos, aber wenn du dein relHaxLog an mich schicken könntest, kann ich es beheben und du wirst diese Nachricht zukuenftig nicht mehr sehen");
			polish.Add("specialMessage1", "Jeśli to widzisz, to znaczy, że masz specificzną konfigurację komputera afektowany przez bug, który nie mogę kopiować na moim systemie. Jest nieszkodliwy, ale jeśli możesz mi przesłać relHaxLog to postaram się naprawić błąd, abyś nie widział tej wiadomości w przyszłości");
					   
            //Componet: 
            //
            english.Add("extractionErrorMessage", "Error deleting the folder res_mods or mods. Either World of Tanks is running or your file and folder security permissions are incorrect");
            german.Add("extractionErrorMessage", "Fehler beim Löschen des Ordners res_mods oder Mods. Entweder World of Tanks läuft oder Ihre Datei- und Ordnersicherheitsberechtigungen sind falsch");
            polish.Add("extractionErrorMessage", "Błąd usuwania folderu res_mods lub mods. Albo World of Tanks jest uruchomione, albo twój plik i folder mają nieprawidłowe zabezpieczenia dostępu");
					   
            //Componet: 
            //
            english.Add("extractionErrorHeader", "Error");
            german.Add("extractionErrorHeader", "Fehler");
            Polish.Add("extractionErrorHeader", "Błąd");

            //Componet: 
            //
            english.Add("deleteErrorHeader", "close out of folders");
            german.Add("deleteErrorHeader", "Ausschliessen von Ordnern");
            polish.Add("deleteErrorHeader", "zamknij foldery");

            //Componet: 
            //
            english.Add("deleteErrorMessage", "Please close all explorer windows in mods or res_mods (or deeper), and click ok to continue.");
            german.Add("deleteErrorMessage", "Bitte schließen Sie alle Explorer-Fenster in Mods oder res_mods (oder Unterordner), und klicken Sie auf OK, um fortzufahren.");
            polish.Add("deleteErrorMessage", "Proszę zamknij folder mods lub res_mods (lub podfoldery), a następnie kliknij kontynuację.");
					   
            //Componet: 
            //
            english.Add("noUninstallLogMessage", "The log file containg the installed files list (installedRelhaxFiles.log) does not exist. Would you like to remove all mods instead?");
            german.Add("noUninstallLogMessage", "Die Protokolldatei mit der Liste der installierten Dateien (installedRelhaxFiles.log) existiert nicht. Möchtest Du alle Mods stattdessen entfernen?");
            polish.Add("noUninstallLogMessage", "Plik logów zawierający listę instalacyjną (installedRelhaxFiles.log) nie istnieje. Czy chciałbyś usunąć modyfikacje?");

            //Componet: 
            //
            english.Add("noUninstallLogHeader", "Remove all mods");
            german.Add("noUninstallLogHeader", "Entferne alle Mods");
	    polish.Add("noUninstallLogHeader", "Usuń wszystkie modyfikacje");

            //Section: Messages from ModSelectionList
            //Componet: 
            //
            english.Add("duplicateMods", "CRITICAL: Duplicate mod name detected");
            german.Add("duplicateMods", "KRITISCH: Duplizierter Modname wurde erkannt");
	    polish.Add("duplicateMods", "BŁĄD KRYTYCZNY: Wykryto zduplikowaną nazwę modyfikacji");

            //Componet: 
            //
            english.Add("databaseReadFailed", "CRITICAL: Failed to read database");
            german.Add("databaseReadFailed", "KRITISCH: Datenbank konnte nicht gelesen werden");
            polish.Add("databaseReadFailed", "BŁĄD KRYTYCZNY: Nie udało się wczytać bazy danych");

            //Componet: 
            //
            english.Add("configSaveSucess", "Config Saved Sucessfully");
            german.Add("configSaveSucess", "Config wurde erfolgreich gespeichert");
            pollish.Add("configSaveSucess", "Udało się zapisać konfigurację");

            //Componet: 
            //
            english.Add("selectConfigFile", "Select a user preference file to load");
            german.Add("selectConfigFile", "Wählen Sie die benutzerdefinierte Datei aus, die geladen werden soll");
            polish.Add("selectConfigFile", "Wybierz plik preferencji do wczytania");

            //Componet: 
            //
            english.Add("configLoadFailed", "The config file could not be loaded, loading in standard mode");
            german.Add("configLoadFailed", "Die Konfigurationsdatei konnte nicht geladen werden, lade im Standard Modus");
            polish.Add("configLoadFailed", "Nie można wczytać pliku knfiguracji, ładowanie trybu podstawowego");

            //Componet: 
            //
            english.Add("modNotFound_1", "The mod, \"");
            german.Add("modNotFound_1", "Der Mod, \"");
            polish.Add("modNotFound_1", "Modyfikacja \"");

            //Componet: 
            //
            english.Add("modNotFound_2", "\" was not found in the modpack. It could have been renamed or removed.");
            german.Add("modNotFound_2", "\" wurde im Modpack nicht gefunden. Er könnte umbenannt oder entfernt worden sein.");
            polish.Add("modNotFound_2", "\" nie została znaleziona w paczce. Sprawdź, czy nie została usunięta lub zmieniona nazwa.");

            //Componet: 
            //
            english.Add("configNotFound_1", "The config \"");
            german.Add("configNotFound_1", "Die Config \"");
            polish.Add("configNotFound_1", "Konfiguracja \"");

            //Componet: 
            //
            english.Add("configNotFound_2", "\" was not found for mod \"");
            german.Add("configNotFound_2", "\" wurde nicht für den Mod gefunden \"");
            polish.Add("configNotFound_2", "\" nie została znaleziona dla modyfikacji \"");

            //Componet: 
            //
            english.Add("configNotFound_3", "\". It could have been renamed or removed.");
            german.Add("configNotFound_3", "\". Er könnte umbenannt oder entfernt worden sein.");
            polish.Add("configNotFound_3", "\". Sprawdź, czy nie została usunięta lub zmieniona nazwa.");

            //Componet: 
            //
            english.Add("prefrencesSet", "preferences Set");
            german.Add("prefrencesSet", "bevorzugte Einstellungen");
            polish.Add("prefrencesSet", "preferowane ustawienia");

            //Componet: 
            //
            english.Add("selectionsCleared", "selections cleared");
            german.Add("selectionsCleared", "Auswahlen geloescht");
            polish.Add("selectionsCleared", "usunięto zaznaczenia");

            //Componet: 
            //
            english.Add("patchingSystemDeneidAccessMessage", "The patching system was denied access to the patch folder. Retry as Administrator. If you see this again, you need to fix your file and folder security permissions");
            german.Add("patchingSystemDeneidAccessMessage", "Dem Patching-System wurde der Zugriff auf den Patch-Ordner verweigert. Wiederholen als Administrator. Wenn Sie dies wieder sehen, müssen Sie Ihre Datei- und Ordnersicherheitsberechtigungen reparieren");
            polish.Add("patchingSystemDeneidAccessMessage", "Nie uzyskano dostępu do folderu patcha. Spróbuj ponownie jako administrator. Jeśli widzisz to ponownie, to zmień ustawienia pozwoleń dostępu do folderów.");

            //Componet: 
            //
            english.Add("patchingSystemDeneidAccessHeader", "Access Deneid");
            german.Add("patchingSystemDeneidAccessHeader", "Zugriff abgelehnt");
            polish.Add("patchingSystemDeneidAccessHeader", "Odmowa dostępu");

            //Componet: 
            //
            english.Add("databaseNotFound", "Database not found at supplied URL");
            german.Add("databaseNotFound", "Datenbank nicht bei der angegebenen URL gefunden");
            polish.Add("databaseNotFound", "Nie znaleziono bazy danych pod wskazanym URL");

            //Componet:
            //
            english.Add("WoTRunningHeader", "WoT is Running");
            german.Add("WoTRunningHeader", "WoT wird gerade ausgeführt.");
            polish.Add("WoTRunningHeader", "WoT jest uruchomiony");

            //Componet:
            //
            english.Add("WoTRunningMessage", "Please close World of Tanks to continue");
            german.Add("WoTRunningMessage", "Um Fortzufahren, schliesse bitte World of Tanks.");
            polish.Add("WoTRunningMessage", "Proszę zamknąć World of Tanks, aby kontynuować");

            //Componet:
            //
            english.Add("InstallingTo", "Installing to");
            german.Add("InstallingTo", "Installiere nach");
            polish.Add("InstallingTo", "Instalowanie w");

            //Componet:
            //
            english.Add("zipReadingErrorHeader", "Incomplete Download");
            german.Add("zipReadingErrorHeader", "Unvollstaendiger Download");
            polish.Add("zipReadingErrorHeader", "Ściąganie niekompletne");

            //Componet:
            //
            english.Add("zipReadingErrorMessage1", "The zip file");
            german.Add("zipReadingErrorMessage1", "Die Zip-Datei");
            Polish.Add("zipReadingErrorMessage1", "Plik skomresowany formatu ZIP ");

            //Componet:
            //
            english.Add("zipReadingErrorMessage2", "konnte nicht gelesen werden, da es hoechstwahrscheinlich ein unvollständiger Download ist. Dies wird uebersprungen. Bitte versuche es spaeter erneut herungerzuladen.");
            german.Add("zipReadingErrorMessage2", "Installiere nach");
            polish.Add("zipReadingErrorMessage2", "nie może zostać odczytany. Sprawdź, czy został prawidłowo pobrany. Instalator pominie modyfikację. Spróbuj ponownie później.");
        }
    }
}
