using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using Minge.FolderSelect;
using ToxicRagers;
using ToxicRagers.Stainless.Formats;
using ToxicRagers.Helpers;
using ToxicRagers.CarmageddonReincarnation.Formats;

namespace VTFucker
{
    public partial class MainForm : Form
    {

        bool VTSelected = false;
        string CurrentVTPath;
        string CarmaFolder;
        List<string> VTFolders = new List<string>();
        Renderer renderer;

        public Renderer Renderer
        {
            get { return renderer; }
            set { renderer = value; }
        }

        BindingSource textureListSource = new BindingSource();
        public BindingSource tileListSource = new BindingSource();
        BindingSource pageNumSource = new BindingSource();

        public MainForm()
        {

            InitializeComponent();

            
            CreateMenus();
            if (Properties.Settings.Default.CarmaDirectory != "" && Directory.Exists(Path.Combine(Properties.Settings.Default.CarmaDirectory, "ZAD_VT")))
            {
                CarmaFolder = Properties.Settings.Default.CarmaDirectory;

            }
            else
            {
                InitialFindFolder();
            }
            ZAD modelsZAD = ZAD.Load(Path.Combine(Environment.CurrentDirectory, "models.zad"));
            //modelsZAD.AddEntryFromBuffer(File.ReadAllBytes("vteffect.fx"), "Data_Core/Content/Models/an_effect.fx");
            var entryToRemove = modelsZAD.Contents[5];
            modelsZAD.ReplaceEntryFromBuffer(entryToRemove, File.ReadAllBytes("vteffect.fx"));
            modelsZAD = ZAD.Load(Path.Combine(Environment.CurrentDirectory,"models.zad")); //*/
            foreach (var entry in modelsZAD.Contents)
            {
                Logger.LogToFile(Logger.LogLevel.Info, "{0}: {1}", modelsZAD.Contents.IndexOf(entry), entry.Name);
            }
            PageType.SelectedIndex = 0;
            PageNumSelect.DataSource = pageNumSource;
            TileListBox.DataSource = tileListSource;
            GetVTList();
        }
        public void InitialFindFolder()
        {
            var folder = FindCarmaFolder();
            if (folder == "")
            {
                var result = MessageBox.Show("Can not find Carmageddon: Reincarnation folder. Click OK to choose a folder or Cancel to quit.", "Game Folder Not Found", MessageBoxButtons.OKCancel);
                if (result == System.Windows.Forms.DialogResult.OK)
                    InitialFindFolder();
                else
                    Application.Exit();

            }
            else
            {
                Properties.Settings.Default.CarmaDirectory = folder;
                CarmaFolder = folder;
                Properties.Settings.Default.Save();
            }
        }
        public string FindCarmaFolder()
        {

            FolderSelectDialog dialog = new FolderSelectDialog();
            dialog.Title = "Select Carmageddon Folder";
            if (dialog.ShowDialog())
            {
                var folder = dialog.FileName;
                if (folder != "" && Directory.Exists(Path.Combine(folder, "ZAD_VT")))
                    return folder;
                else
                {
                    MessageBox.Show("This is not a valid Carmageddon: Reincarnation folder. Please choose a different one.", "Invalid Folder", MessageBoxButtons.OK);
                    return FindCarmaFolder();
                }
            }
            else return "";
        }
        public void CreateMenus()
        {
            var menu = new MainMenu();
            menu.MenuItems.Add("File");
            menu.MenuItems.Add("Edit");
            menu.MenuItems[0].MenuItems.Add("&Open Carma Folder", OpenCarmaFolder_Click);
            menu.MenuItems[0].MenuItems.Add("&Load VT");
            menu.MenuItems[0].MenuItems[1].MenuItems.Add("&Select VT Folder", SelectVTFolder_Click);
            menu.MenuItems[0].MenuItems.Add("&Exit", (o, e) => { Application.Exit(); });

            menu.MenuItems[1].MenuItems.Add("Export Selected Texture", ExportSelectedTexture_Click);
            menu.MenuItems[1].MenuItems.Add("Export Selected Texture's Tiles", ExportSelectedTextureTiles_Click);
            menu.MenuItems[1].MenuItems.Add("Export Current Page", ExportSelectedPage_Click);
            menu.MenuItems[1].MenuItems.Add("Replace Selected Texture", ReplaceSelectedTexture_Click);
            menu.MenuItems[1].MenuItems.Add("Shader Editor", StartShaderEditor_Click);

            Menu = menu;
        }

        void StartShaderEditor_Click(object sender, EventArgs e)
        {
            if (renderer == null) return;
            ShaderEditorForm f = new ShaderEditorForm(File.ReadAllText("vteffect.fx"), renderer);
            f.Show();
        }
        void OpenCarmaFolder_Click(object sender, EventArgs e)
        {
            var folder = FindCarmaFolder();
            if (folder != "")
            {
                Properties.Settings.Default.CarmaDirectory = folder;
                CarmaFolder = folder;
                Properties.Settings.Default.Save();
                GetVTList();
            }
        }
        void SelectVTFolder_Click(object sender, EventArgs e)
        {

        }
        void ExportSelectedPage_Click(object sender, EventArgs e)
        {
            var dialog = new Minge.FolderSelect.FolderSelectDialog();
            dialog.Title = "Export VT Page To...";
            dialog.InitialDirectory = Properties.Settings.Default.LastExportDirectory;
            var result = dialog.ShowDialog();
            if (result)
            {
                if (Directory.Exists(dialog.FileName))
                {
                    Properties.Settings.Default.LastExportDirectory = dialog.FileName;
                    Properties.Settings.Default.Save();
                    List<crVTMapEntry> entries = null;
                    if (PageType.SelectedItem == "Diffuse") entries = DiffuseMap.Entries;
                    else if (PageType.SelectedItem == "Specular") entries = SpecularMap.Entries;
                    else if (PageType.SelectedItem == "Normal") entries = NormalMap.Entries;
                    crVTPage vtPage = (crVTPage)PageNumSelect.SelectedItem;

                    int numTextures = 0;
                    foreach (var entry in entries)
                    {
                        SaveTexture(entry, Path.Combine(dialog.FileName, Path.GetFileNameWithoutExtension(entry.FileName) + ".png"), vtPage);
                        numTextures++;
                    }

                    MessageBox.Show(numTextures + " Textures Saved!", "Save Complete", MessageBoxButtons.OK);
                }
            }
        }
        void ExportSelectedTextureTiles_Click(object sender, EventArgs e)
        {
            var textureToExport = (crVTMapEntry)TextureList.SelectedItem;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = Properties.Settings.Default.LastExportDirectory;
            dialog.Title = "Export Texture";
            dialog.Filter = "PNG Image (*.PNG)|*.png|JPEG Image (*.JPG, *.JPEG)|*.jpg;*.jpeg|BMP Image (*.BMP)|*.bmp|TIFF Image (*.TIF, *.TIFF)|*.tif;*.tiff|TGA Image (*.TGA)|*.tga|TDX Texture (*.TDX)|*.tdx";
            dialog.FileName = Path.GetFileName(textureToExport.FileName);
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && Directory.Exists(Path.GetDirectoryName(dialog.FileName)))
            {
                SaveTextureTiles(textureToExport, dialog.FileName);
                MessageBox.Show("Texture Tiles Saved!", "Save Complete", MessageBoxButtons.OK);
            }
        }
        void ExportSelectedTexture_Click(object sender, EventArgs e)
        {
            var textureToExport = (crVTMapEntry)TextureList.SelectedItem;
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = Properties.Settings.Default.LastExportDirectory;
            dialog.Title = "Export Texture";
            dialog.Filter = "PNG Image (*.PNG)|*.png|JPEG Image (*.JPG, *.JPEG)|*.jpg;*.jpeg|BMP Image (*.BMP)|*.bmp|TIFF Image (*.TIF, *.TIFF)|*.tif;*.tiff|TGA Image (*.TGA)|*.tga|TDX Texture (*.TDX)|*.tdx";
            dialog.FileName = Path.GetFileName(textureToExport.FileName);
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && Directory.Exists(Path.GetDirectoryName(dialog.FileName)))
            {
                SaveTexture(textureToExport, dialog.FileName);
                MessageBox.Show("Texture Saved!", "Save Complete", MessageBoxButtons.OK);
            }
        }

        private void SaveTexture(crVTMapEntry textureToExport, String FileName)
        {
            SaveTexture(textureToExport, FileName, (crVTPage)PageNumSelect.SelectedItem);
        }
        private void SaveTexture(crVTMapEntry textureToExport, String FileName, crVTPage vtPage)
        {
            Properties.Settings.Default.LastExportDirectory = Path.GetDirectoryName(FileName);
            Properties.Settings.Default.Save();

            var fileType = Path.GetExtension(FileName).ToUpper();
            ImageFormat imgFormat = ImageFormat.Png;

            if (fileType == ".TGA")
            {
                vtPage.SaveTexture(textureToExport, FileName, false, true, false, ImageFormat.Png);
            }
            else if (fileType == ".TDX")
            {
                vtPage.SaveTexture(textureToExport, FileName, true, false, false, ImageFormat.Png);
            }
            else
            {
                switch (fileType)
                {
                    case ".JPG":
                        imgFormat = ImageFormat.Jpeg;
                        break;
                    case ".JPEG":
                        imgFormat = ImageFormat.Jpeg;
                        break;
                    case ".TIFF":
                        imgFormat = ImageFormat.Tiff;
                        break;
                    case ".TIF":
                        imgFormat = ImageFormat.Tiff;
                        break;
                    case ".BMP":
                        imgFormat = ImageFormat.Bmp;
                        break;
                    case ".PNG":
                        imgFormat = ImageFormat.Png;
                        break;
                }
                vtPage.SaveTexture(textureToExport, FileName, imgFormat);
            }
        }
        private void SaveTextureTiles(crVTMapEntry textureToExport, String FileName)
        {
            Properties.Settings.Default.LastExportDirectory = Path.GetDirectoryName(FileName);
            Properties.Settings.Default.Save();

            var fileType = Path.GetExtension(FileName).ToUpper();

            var path = Path.GetDirectoryName(FileName);
            var filenameprefix = Path.GetFileNameWithoutExtension(FileName);
            ImageFormat imgFormat = ImageFormat.Png;

            List<crVTPage> vtPages = diffusePages;
            if (PageType.SelectedItem == "Specular") vtPages = specularPages;
            else if (PageType.SelectedItem == "Normal") vtPages = normalPages;

            foreach (var vtPage in vtPages)
            {
                var tiles = vtPage.GetTiles(textureToExport);
                int tileNum = 0;
                foreach (var tile in tiles)
                {
                    if (tile.TDXTile.Texture == null) tile.TDXTile.GetTextureFromZAD();
                    tileNum++;
                    string tileFileName = Path.Combine(path, filenameprefix + "_p"+vtPage.PageNum+ "_t" + String.Format("{0}", tileNum) + fileType);
                    if (fileType == ".TGA")
                    {
                        //vtPage.SaveTexture(textureToExport, FileName, false, true, false, ImageFormat.Png);
                    }
                    else if (fileType == ".TDX")
                    {
                        ZAD tilezad = ZAD.Load(tile.TDXTile.ZADFile);
                        tilezad.Extract(tilezad.Contents.Find(entry => entry.Name == tile.TDXTile.ZADEntryLocation), path+"/");
                        //tile.TDXTile.Texture.Save(tileFileName);
                        //vtPage.SaveTexture(textureToExport, FileName, true, false, false, ImageFormat.Png);
                    }
                    else
                    {
                        switch (fileType)
                        {
                            case ".JPG":
                                imgFormat = ImageFormat.Jpeg;
                                break;
                            case ".JPEG":
                                imgFormat = ImageFormat.Jpeg;
                                break;
                            case ".TIFF":
                                imgFormat = ImageFormat.Tiff;
                                break;
                            case ".TIF":
                                imgFormat = ImageFormat.Tiff;
                                break;
                            case ".BMP":
                                imgFormat = ImageFormat.Bmp;
                                break;
                            case ".PNG":
                                imgFormat = ImageFormat.Png;
                                break;
                        }
                        Bitmap b = tile.TDXTile.Texture.Decompress(0, false);
                        b.Save(tileFileName);
                    }
                }
            }
        }
        void ReplaceSelectedTexture_Click(object sender, EventArgs e)
        {

            var textureToReplace = (crVTMapEntry)TextureList.SelectedItem;
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.InitialDirectory = Properties.Settings.Default.LastImportDirectory;
            dialog.Title = "Export Texture";
            dialog.Filter = "PNG Image (*.PNG)|*.png|JPEG Image (*.JPG, *.JPEG)|*.jpg;*.jpeg|BMP Image (*.BMP)|*.bmp|TIFF Image (*.TIF, *.TIFF)|*.tif;*.tiff|TGA Image (*.TGA)|*.tga|TDX Texture (*.TDX)|*.tdx";
            dialog.FileName = Path.GetFileName(textureToReplace.FileName);
            var result = dialog.ShowDialog();
            if (result == System.Windows.Forms.DialogResult.OK && Directory.Exists(Path.GetDirectoryName(dialog.FileName)))
            {
                Properties.Settings.Default.LastImportDirectory = Path.GetDirectoryName(dialog.FileName);
                Properties.Settings.Default.Save();
                var vtPage = (crVTPage)PageNumSelect.SelectedItem;
                var fileType = Path.GetExtension(dialog.FileName).ToUpper();
                ImageFormat imgFormat = ImageFormat.Png;
                Bitmap image = null;
                if (fileType == ".TGA")
                {
                    image = Paloma.TargaImage.LoadTargaImage(dialog.FileName);
                }
                else if (fileType == ".TDX")
                {
                    image = TDX.Load(dialog.FileName).Decompress();
                }
                else
                {
                    image = (Bitmap)Bitmap.FromFile(dialog.FileName);
                }
                //if (image.Width != textureToReplace.GetWidth(vtPage.PageNum) || image.Height != textureToReplace.GetHeight(vtPage.PageNum)) MessageBox.Show("Error: Image dimensions need to match the original texture!", "Texture Size Mismatch", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //else
                {
                    List<crVTPage> pages = null;
                    if (PageType.SelectedItem == "Diffuse") pages = diffusePages;
                    else if (PageType.SelectedItem == "Specular") pages = specularPages;
                    else if (PageType.SelectedItem == "Normal") pages = normalPages;
                    foreach (var page in pages)
                    {
                        if (page.PageNum == 0) continue;
                        int targetWidth = textureToReplace.GetWidth(page.PageNum);
                        int targetHeight = textureToReplace.GetHeight(page.PageNum);
                        Bitmap mipimage = null;
                        if (image.Width != targetWidth || image.Height != targetHeight)
                        {

                            mipimage = new Bitmap(targetWidth, targetHeight);
                            var srcRect = new RectangleF(0, 0, image.Width, image.Height);
                            var destRect = new RectangleF(0, 0, targetWidth, targetHeight);
                            Graphics grfx = Graphics.FromImage(mipimage);
                            grfx.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                            grfx.DrawImage(image, destRect, srcRect, GraphicsUnit.Pixel);

                        }
                        else mipimage = image;
                        var tiles = page.ImportTexture(mipimage, textureToReplace);
                        foreach(var tile in tiles)
                        {
                            ZAD currentZAD = ZAD.Load(tile.ZADFile);
                            ZADEntry tileEntry = currentZAD.Contents.Find(possibleEntry => possibleEntry.Name == tile.ZADEntryLocation);
                            currentZAD.ReplaceEntryFromBuffer(tileEntry, tile.Texture.SaveToBuffer());
                        }
                        /*foreach(var tile in tiles)
                        {
                            using (ZipArchive zip = ZipFile.Open(tile.ZADFile, ZipArchiveMode.Update))
                            {
                                
                                var zipentry = zip.GetEntry(tile.ZADEntryLocation);
                                zipentry.Delete();
                                zipentry = zip.CreateEntry(tile.ZADEntryLocation, CompressionLevel.NoCompression);
                                using (Stream stream = zipentry.Open())
                                {
                                    tile.Texture.Save(stream);
                                }
                                
                            }
                        }*/
                    }
                }
            }
        }

        void GetVTList()
        {
            var folders = Directory.GetDirectories(Path.Combine(CarmaFolder, "ZAD_VT"));
            var SelectVTFolderMenuItem = Menu.MenuItems[0].MenuItems[1].MenuItems[0];
            Menu.MenuItems[0].MenuItems[1].MenuItems.Clear();
            Menu.MenuItems[0].MenuItems[1].MenuItems.Add(SelectVTFolderMenuItem);

            Menu.MenuItems[0].MenuItems[1].MenuItems.Add("-");
            VTFolders.Clear();
            foreach (var folder in folders)
            {
                if (!File.Exists(Path.Combine(folder, "Environments.zad"))) continue;
                VTFolders.Add(folder);

                Menu.MenuItems[0].MenuItems[1].MenuItems.Add(Path.GetFileName(folder));
                Menu.MenuItems[0].MenuItems[1].MenuItems[Menu.MenuItems[0].MenuItems[1].MenuItems.Count - 1].Click += (s, e) => { LoadVT(folder); };
            }
        }

        TDX LoadTDXFromZADEntry(ZADEntry entry, ZAD zadFile)
        {
            using (MemoryStream stream = new MemoryStream(zadFile.ExtractToBuffer(entry)))
            {
                TDX output = TDX.LoadFromMemoryStream(stream, entry.Name);
                return output;
            }

        }
        List<crVTPage> diffusePages;
        List<crVTPage> normalPages;
        List<crVTPage> specularPages;
        crVTMap DiffuseMap;

        crVTMap SpecularMap;

        crVTMap NormalMap;
        void LoadVT(string inputFolder)
        {
            var zadFiles = Directory.EnumerateFiles(inputFolder, "*.zad");
            bool firstZad = true;
            ZAD EnvironmentsZAD = ZAD.Load(Path.Combine(inputFolder, "Environments.zad"));
            TDX DiffuseTDX = null;
            TDX SpecularTDX = null;
            TDX NormalTDX = null;
            Console.WriteLine("Loading dictionary TDX files...");
            foreach (ZADEntry entry in EnvironmentsZAD.Contents)
            {
                Console.WriteLine(entry.Name);
                switch (Path.GetFileName(entry.Name).ToLower())
                {
                    case "diffuse_d.tdx":
                        DiffuseTDX = LoadTDXFromZADEntry(entry, EnvironmentsZAD);
                        break;
                    case "specular_s.tdx":
                        SpecularTDX = LoadTDXFromZADEntry(entry, EnvironmentsZAD);
                        break;
                    case "normal_n.tdx":
                        NormalTDX = LoadTDXFromZADEntry(entry, EnvironmentsZAD);
                        break;
                }
            }

            DiffuseMap = (crVTMap)DiffuseTDX.ExtraData;

            SpecularMap = (crVTMap)SpecularTDX.ExtraData;

            NormalMap = (crVTMap)NormalTDX.ExtraData;

            textureListSource.DataSource = DiffuseMap.Entries;
            TextureList.DataSource = textureListSource;
            diffusePages = new List<crVTPage>();
            for (int i = 0; i < DiffuseMap.PageCount + 1; i++)
            {
                int pageWidth = DiffuseMap.GetWidth(i);
                int pageHeight = DiffuseMap.GetHeight(i);
                diffusePages.Add(new crVTPage(pageWidth, pageHeight, i, DiffuseMap));
                Console.WriteLine("\tDiffuse Page {0} created", i);
            }
            specularPages = new List<crVTPage>();
            for (int i = 0; i < SpecularMap.PageCount + 1; i++)
            {
                int pageWidth = SpecularMap.GetWidth(i);
                int pageHeight = SpecularMap.GetHeight(i);
                specularPages.Add(new crVTPage(pageWidth, pageHeight, i, SpecularMap));
                Console.WriteLine("\tDiffuse Page {0} created", i);
            }
            normalPages = new List<crVTPage>();
            for (int i = 0; i < NormalMap.PageCount + 1; i++)
            {
                int pageWidth = NormalMap.GetWidth(i);
                int pageHeight = NormalMap.GetHeight(i);
                normalPages.Add(new crVTPage(pageWidth, pageHeight, i, NormalMap));
                Console.WriteLine("\tDiffuse Page {0} created", i);
            }
            if (PageType.SelectedItem == "Diffuse") pageNumSource.DataSource = diffusePages;
            else if (PageType.SelectedItem == "Specular") pageNumSource.DataSource = specularPages;
            else if (PageType.SelectedItem == "Normal") pageNumSource.DataSource = normalPages;
            PageNumSelect.SelectedIndex = 1;

            foreach (string zadFile in zadFiles)
            {
                if (Path.GetFileNameWithoutExtension(zadFile).ToLower() == "environments") continue;
                //Console.Write("Loading ZAD: " + zadFile);
                /*if(Path.GetFileNameWithoutExtension(zadFile).ToLower() == "pages_5")
                {
                    Console.WriteLine("This is page 5");
                }*/
                ZAD currentZAD = ZAD.Load(zadFile);

                foreach (ZADEntry entry in currentZAD.Contents)
                {
                    if (entry.CompressionMethod !=  CompressionMethods.LZ4)
                    {
                        //Console.WriteLine("This entry isnt compressed using lz4! wtf? {0}", entry.Name);

                    }
                    string tdxName = Path.GetFileNameWithoutExtension(entry.Name).ToLower();
                    string tileName = tdxName.Split(new Char[] { '_' })[0].ToUpper();
                    /*if (tileName == "E4C7607E")
                    {
                        Console.WriteLine("This is E4C7607E");
                    }*/
                    if (DiffuseMap.TilesByName.ContainsKey(tileName))
                    {

                        crVTMapTileTDX tileTDX = DiffuseMap.TilesByName[tileName];
                        //tileTDX.Texture = LoadTDXFromZADEntry(entry, currentZAD);
                        tileTDX.ZADFile = zadFile;
                        tileTDX.ZADEntryLocation = entry.Name;
                        for (int i = 0; i < tileTDX.Coords.Count; i++)
                        {
                            crVTMapTile tile = tileTDX.Coords[i];
                            tile.TDXTile = tileTDX;
                            //if (tile.Row < diffusePages[tile.Page].maxTilesToStitch && tile.Column < diffusePages[tile.Page].maxTilesToStitch)
                            {
                                diffusePages[tile.Page].Tiles[tile.Row][tile.Column] = tileTDX;// LoadTDXFromZADEntry(entry, currentZAD);
                            }
                        }
                    }
                    if (SpecularMap.TilesByName.ContainsKey(tileName))
                    {

                        crVTMapTileTDX tileTDX = SpecularMap.TilesByName[tileName];
                        //tileTDX.Texture = LoadTDXFromZADEntry(entry, currentZAD);
                        tileTDX.ZADFile = zadFile;
                        tileTDX.ZADEntryLocation = entry.Name;
                        for (int i = 0; i < tileTDX.Coords.Count; i++)
                        {
                            crVTMapTile tile = tileTDX.Coords[i];
                            tile.TDXTile = tileTDX;
                            if (tile.Row < specularPages[tile.Page].maxTilesToStitch && tile.Column < specularPages[tile.Page].maxTilesToStitch)
                            {
                                specularPages[tile.Page].Tiles[tile.Row][tile.Column] = tileTDX;// LoadTDXFromZADEntry(entry, currentZAD);
                            }
                        }
                    }
                    if (NormalMap.TilesByName.ContainsKey(tileName))
                    {
                        //currentZAD.Extract(entry, Path.Combine(outputFolder, "Normal", "TDX")+"/");
                        crVTMapTileTDX tileTDX = NormalMap.TilesByName[tileName];
                        //tileTDX.Texture = LoadTDXFromZADEntry(entry, currentZAD);
                        tileTDX.ZADFile = zadFile;
                        tileTDX.ZADEntryLocation = entry.Name;
                        for (int i = 0; i < tileTDX.Coords.Count; i++)
                        {
                            crVTMapTile tile = tileTDX.Coords[i];
                            tile.TDXTile = tileTDX;
                            if (tile.Row < normalPages[tile.Page].maxTilesToStitch && tile.Column < normalPages[tile.Page].maxTilesToStitch)
                            {
                                normalPages[tile.Page].Tiles[tile.Row][tile.Column] = tileTDX;// LoadTDXFromZADEntry(entry, currentZAD);
                            }
                        }
                    }
                }
                
                if (firstZad && 1 == 0)
                {

                    firstZad = false;
                    foreach (var vtentry in DiffuseMap.Entries)
                    {
                        bool breakloops = false;
                        foreach (var vtpage in diffusePages)
                        {
                            var tiles = vtpage.GetTiles(vtentry);
                            foreach (var vttile in tiles)
                            {
                                if (vttile.TDXTile.ZADFile == zadFile)
                                {
                                    Logger.LogToFile( Logger.LogLevel.Debug, "{0} = {1} (Diffuse / Page {2})", vttile.TDXTile.ZADEntryLocation, vtentry.FileName, vttile.Page);
                                    // breakloops = true;
                                    //break;
                                }
                            }
                            //if (breakloops) break;
                        }
                        //if (breakloops) break;
                    }

                    foreach (var vtentry in SpecularMap.Entries)
                    {
                        bool breakloops = false;
                        foreach (var vtpage in specularPages)
                        {
                            var tiles = vtpage.GetTiles(vtentry);
                            foreach (var vttile in tiles)
                            {
                                if (vttile.TDXTile.ZADFile == zadFile)
                                {
                                    Logger.LogToFile(Logger.LogLevel.Debug, "{0} = {1} (Specular / Page {2})", vttile.TDXTile.ZADEntryLocation, vtentry.FileName, vttile.Page);
                                    //breakloops = true;
                                    //break;
                                }
                            }
                            //if (breakloops) break;
                        }
                        //if (breakloops) break;
                    }

                    foreach (var vtentry in NormalMap.Entries)
                    {
                        bool breakloops = false;
                        foreach (var vtpage in normalPages)
                        {
                            var tiles = vtpage.GetTiles(vtentry);
                            foreach (var vttile in tiles)
                            {
                                if (vttile.TDXTile.ZADFile == zadFile)
                                {
                                    Logger.LogToFile(Logger.LogLevel.Debug, "{0} = {1} (Normal / Page {2})", vttile.TDXTile.ZADEntryLocation, vtentry.FileName, vttile.Page);
                                    //breakloops = true;
                                    //break;
                                }
                            }
                            // if (breakloops) break;
                        }
                        //if (breakloops) break;
                    }
                }
                //Thread zadThread = new Thread(Program.ExtractZADContent);
                //zadThread.Start(currentZAD);
                //Threads.Add(zadThread);
                //ThreadsAlive++;
                //break;
            }
            TextureList.SetSelected(0, true);
        }

        private void TextureList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (diffusePages == null) return;
            var entry = (crVTMapEntry)TextureList.SelectedItem;
            renderer.VTPage = (crVTPage)PageNumSelect.SelectedItem;// diffusePages[1];
            TextureName.Text = Path.GetFileName(entry.FileName);
            NumTiles.Text = entry.NumTiles.ToString();
            TextureDimensions.Text = entry.Width.ToString() + "px X " + entry.Height.ToString() + "px";
            renderer.SetTexture(entry);


        }

        private void TileListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tile = (crVTMapTile)TileListBox.SelectedItem;
            TilePreviewBox.Image = tile.TDXTile.Texture.Decompress(0, true);
        }

        private void PageType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (PageType.SelectedItem == "Diffuse")
            {
                pageNumSource.DataSource = diffusePages;
                textureListSource.DataSource = DiffuseMap != null ? DiffuseMap.Entries : null;
            }
            else if (PageType.SelectedItem == "Specular")
            {
                pageNumSource.DataSource = specularPages;
                textureListSource.DataSource = SpecularMap != null? SpecularMap.Entries : null;
            }
            else if (PageType.SelectedItem == "Normal")
            {
                pageNumSource.DataSource = normalPages;

                textureListSource.DataSource = NormalMap != null ? NormalMap.Entries : null;
            }
            if (TextureList.SelectedIndex >= 0)
                TextureList.SetSelected(TextureList.SelectedIndex, true);
        }

        private void PageNumSelect_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            TextureList.SetSelected(TextureList.SelectedIndex, true);
        }
    }
}
