using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using Microsoft.Win32;
using GifImageLib;

namespace GifSeparator
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
      private BitmapDecoder bd = null;
      public MainWindow()
      {
         InitializeComponent();
      }

      private void ShowGifInfo(byte[] buffer, string imgPath)
      {
         string type = ASCIIEncoding.ASCII.GetString(buffer, 0, 3); //前3个字节，标识"GIF"
         string version = ASCIIEncoding.ASCII.GetString(buffer, 3, 3);//3~6个字节，版本号
         int logicalWidth = BitConverter.ToUInt16(buffer, 6);//第7，8两个字节，宽度
         int logicalHeight = BitConverter.ToUInt16(buffer, 8); //第9，10两个字节，高度

         txtFileName.Text = "图片路径: " + imgPath;
         txtTotalFrames.Text = "总帧数: " + bd.Frames.Count.ToString();
         txtRealHeight.Text = "实际高度: " + logicalHeight.ToString() + "px";
         txtRealWidth.Text = "实际宽度: " + logicalWidth.ToString() + "px";
         txtVersion.Text = "Gif版本: " + version;
      }

      public void ShowGif(string _imgPath)
      {
         FileStream fsImg = new FileStream(_imgPath, FileMode.Open);
         MemoryStream gif = new MemoryStream();
         fsImg.CopyTo(gif);
         fsImg.Close();

         byte[] buffer = new byte[gif.Length];
         gif.Position = 0;
         gif.Read(buffer, 0, Convert.ToInt32(gif.Length));
         //判断图片是否为GIF格式，通过图片的前三个字节标识位来判断
         if (!ASCIIEncoding.ASCII.GetString(buffer, 0, 3).ToLower().Equals("gif"))
         {
            MessageBox.Show("无效的GIF图片！");
            return;
         }
         //清空容器
         spFrames.Children.Clear();
         spGifImage.Children.Clear();

         bd = new GifBitmapDecoder(gif, BitmapCreateOptions.None, BitmapCacheOption.Default);
         ShowGifInfo(buffer, _imgPath); //显示图片的信息
         //显示图片的所有帧
         for (int i = 0; i < bd.Frames.Count(); i++)
         {
            System.Windows.Controls.Image img = new System.Windows.Controls.Image();
            img.Source = bd.Frames[i];
            img.Width = 200;
            img.Height = 200;
            img.Margin = new Thickness(6, 0, 0, 0);
            spFrames.Children.Add(img);
         }
         GifImage gifImage = new GifImage();
         gifImage.Source = _imgPath;
         spGifImage.Children.Add(gifImage);
      }

      private void Open_Click(object sender, RoutedEventArgs e)
      {
         OpenFileDialog open = new OpenFileDialog();
         if (open.ShowDialog() == true)
         {
            ShowGif(open.FileName);
         }
      }

      private void Save_Click(object sender, RoutedEventArgs e)
      {
         if(bd == null) return;
         SaveFileDialog save = new SaveFileDialog();
         if (save.ShowDialog() == true)
         {
            for (int i = 0; i < bd.Frames.Count(); i++)
            {
               FileStream stream = new FileStream(save.FileName + i + ".png", FileMode.Create);
               PngBitmapEncoder encoder = new PngBitmapEncoder();
               encoder.Frames.Add(bd.Frames[i]);
               encoder.Save(stream);
               stream.Close();
            }
            MessageBox.Show("保存成功！");
         }
      }
    }
}


