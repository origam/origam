#region license
/*
Copyright 2005 - 2021 Advantage Solutions, s. r. o.

This file is part of ORIGAM (http://www.origam.org).

ORIGAM is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

ORIGAM is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with ORIGAM. If not, see <http://www.gnu.org/licenses/>.
*/
#endregion

using System.Drawing;

namespace Origam.Workbench;

public class Images
{
    private static Image[] images = null;

    // ImageList.Images[int index] does not preserve alpha channel.
    static Images()
    {
        // TODO alpha channel PNG loader is not working on .NET Service RC1
        System.Reflection.Assembly thisExe;
        thisExe = System.Reflection.Assembly.GetExecutingAssembly();
        System.IO.Stream file = thisExe.GetManifestResourceStream(
            "Origam.Workbench.BaseComponents.ImageList16.png"
        );
        Bitmap bitmap = (Bitmap)Bitmap.FromStream(file);
        bitmap.MakeTransparent(Color.Magenta);
        int count = (int)(bitmap.Width / bitmap.Height);
        images = new Image[count];
        Rectangle rectangle = new Rectangle(0, 0, bitmap.Height, bitmap.Height);
        for (int i = 0; i < count; i++)
        {
            images[i] = bitmap.Clone(rectangle, bitmap.PixelFormat);
            rectangle.X += bitmap.Height;
        }
    }

    public static Image New => images[0];
    public static Image Open => images[1];
    public static Image Save => images[2];
    public static Image Cut => images[3];
    public static Image Copy => images[4];
    public static Image Paste => images[5];
    public static Image Delete => images[6];
    public static Image Properties => images[7];
    public static Image Undo => images[8];
    public static Image Redo => images[9];
    public static Image Preview => images[10];
    public static Image Print => images[11];
    public static Image Search => images[12];
    public static Image ReSearch => images[13];
    public static Image Help => images[14];
    public static Image ZoomIn => images[15];
    public static Image ZoomOut => images[16];
    public static Image Back => images[17];
    public static Image Forward => images[18];
    public static Image Favorites => images[19];
    public static Image AddToFavorites => images[20];
    public static Image Stop => images[21];
    public static Image Refresh => images[22];
    public static Image Home => images[23];
    public static Image Edit => images[24];
    public static Image Tools => images[25];
    public static Image Tiles => images[26];
    public static Image Icons => images[27];
    public static Image List => images[28];
    public static Image Details => images[29];
    public static Image Pane => images[30];
    public static Image Culture => images[31];
    public static Image Languages => images[32];
    public static Image History => images[33];
    public static Image Mail => images[34];
    public static Image Parent => images[35];
    public static Image FolderProperties => images[36];
    public static Image SaveToDatabase => images[37];
    public static Image RecurringWorkflow => images[38];
    public static Image DeploymentScriptGenerator => images[39];
    public static Image ExtensionBrowser => images[40];
    public static Image SchemaBrowser => images[41];
    public static Image PropertyPad => images[42];
    public static Image FindSchemaItemResults => images[43];
    public static Image Output => images[44];
    public static Image ConnectionConfiguration => images[45];
    public static Image HelpCircle => images[46];
    public static Image Attachment => images[47];
    public static Image Lock => images[48];
    public static Image Accept => images[49];
    public static Image Cancel => images[50];
    public static Image GlobalTransactionHistoryPad => images[51];
    public static Image RestartServer => images[52];
    public static Image Git => images[53];
}
