using Microsoft.Win32;
using System.Windows;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;
using System.Text.RegularExpressions;

namespace MapGenerator;

public partial class MainWindow : Window
{

    private readonly string ImagesUri = "Data/Images/CellType";

    public MainWindow()
    {
        InitializeComponent();
    }

    private void MapGeneration(object sender, RoutedEventArgs e)
    {
        Random rand = new Random();

        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                (FindName("Image" + (i + 1) + (j + 1)) as System.Windows.Controls.Image).Source =
                    new BitmapImage(new Uri(ImagesUri + rand.Next(1, 6) + ".png", UriKind.Relative))
                    { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
            }
        }
    }

    private void MapSaving(object sender, RoutedEventArgs e)
    {
        if (Image11.Source == null)
        {
            MessageBox.Show("Сначало сгенерируйте карту.");
            return;
        }

        SaveFileDialog save = new SaveFileDialog();

        save.Title = "Save picture as ";
        save.Filter = "Image Files(*.png)|*.png";
        save.FileName = "Map-" + Regex.Replace(DateTime.Now.ToString(), "[.: ]", "");

        if (save.ShowDialog() == true)
        {
            List<BitmapImage> Images = new List<BitmapImage>();

            for (int i = 0; i < 6; i++)
            {
                for (int j = 0; j < 6; j++)
                {
                    Images.Add((BitmapImage)(FindName("Image" + (i + 1) + (j + 1)) as System.Windows.Controls.Image).Source);
                }
            }

            Generation(Images, save.FileName);
        }
    }

    private BitmapSource Generation(List<BitmapImage> images, string filePath)
    {

        // Определяем высоту и ширину объединенного изображения
        int totalWidth = (int)images.Last().PixelWidth * 6;


        // Создаем RenderTargetBitmap для объединенного изображения
        RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap(totalWidth, totalWidth, 96, 96, PixelFormats.Pbgra32);

        // Создаем визуализатор для отрисовки сцен
        DrawingVisual visual = new DrawingVisual();
        using (DrawingContext drawingContext = visual.RenderOpen())
        {
            int xOffset = 0;
            int yOffset = 0;

            foreach (var image in images)
            {
                drawingContext.DrawImage(image, new Rect(xOffset, yOffset, image.PixelWidth, image.PixelHeight));

                // Смещаем начальную позицию следующего изобрашения по горизонтали на ширину предыдущего
                xOffset += (int)image.PixelWidth;

                // Если дошли до конца строки то смещаем начальную поицию по вертикали на высоту предыдущей строки и обнуляем начало по горизонтали
                if (xOffset == (int)image.PixelWidth * 6) { yOffset += image.PixelWidth; xOffset = 0; }
            }
        }

        // Рендерим визуализатор в RenderTargetBitmap
        renderTargetBitmap.Render(visual);

        PngBitmapEncoder pngEncoder = new PngBitmapEncoder();
        pngEncoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

        // Сохраняем в файл
        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            pngEncoder.Save(fileStream);
        }

        return renderTargetBitmap;
    }

    private void RegenerationMapCell(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        (sender as System.Windows.Controls.Image).Source = new BitmapImage(new Uri(ImagesUri + new Random().Next(1, 6) + ".png", UriKind.Relative))
        { CreateOptions = BitmapCreateOptions.IgnoreImageCache };
    }
}
