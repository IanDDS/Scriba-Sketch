/* Copyright (c) 2016 Microsoft Corporation. This software is licensed under the MIT License.
 * See the license file delivered with this project for further information.
 */
using System;
using System.Threading;
using System.Collections.Generic;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using HeartBeat.Engine;
using HeartBeat.Controls;
using Windows.ApplicationModel.Background;
using Windows.System.Threading;
using Windows.UI.Core;
using Windows.UI.Xaml.Input;
using Windows.UI.Notifications;
using Windows.UI.Input.Inking;
using Windows.Foundation;
using Windows.UI.Input;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media;
using Windows.UI;
using Windows.Devices.Input;
using System.Linq;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Graphics.Display;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace HeartBeat
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class HeartBeatPage : Page
    {
        private bool _UseSimlator = false;
        public static List<int> myValues = new List<int>();
        public static List<int> avgValues2;
        public static int me;
        public static int avg;

        //Canvas variables
        InkManager _inkKhaled = new Windows.UI.Input.Inking.InkManager();
        private uint _penID;
        private uint _touchID;
        private Point _previousContactPt;
        private Point currentContactPt;
        private double x1;
        private double y1;
        private double x2;
        private double y2;
        public static float fin;
        public static float pressureNorm;
        public float value = 10;
        UIElement line;
        Line l;
        int num = 0;

        private Color m_CurrentColour = Colors.Black;
        SolidColorBrush brush = new SolidColorBrush(Colors.Red);
        InkManager m_InkManager = new Windows.UI.Input.Inking.InkManager();
        private Stack<Line> _undo = new Stack<Line>();
        private Stack<Line> _redo = new Stack<Line>();

        public HeartBeatPage()
        {
            this.InitializeComponent();

            //m_CanvasManager = new CanvasManager(MyCanvas);

            MyCanvas.PointerPressed += new PointerEventHandler(MyCanvas_PointerPressed);
            MyCanvas.PointerMoved += new PointerEventHandler(MyCanvas_PointerMoved);
            MyCanvas.PointerReleased += new PointerEventHandler(MyCanvas_PointerReleased);
            MyCanvas.PointerExited += new PointerEventHandler(MyCanvas_PointerReleased);

        }
        
        public InkManager CurrentManager
        {
            get
            {
                return _inkKhaled;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            base.OnNavigatedTo(e);

            var parameter = e.Parameter as String;

            if (parameter != null && parameter.Equals("simulator"))
            {
                _UseSimlator = true;
            }
            else
            {
                _UseSimlator = false;
            }

            if (HeartBeatEngine.Instance.SelectedDevice == null && !_UseSimlator)
            {
                ShowErrorDialog("Please select device !", "No device selected");
                return;
            }

            SetWaitVisibility(true);

            progressIndicator.Text = "Connecting...";

            System.Diagnostics.Debug.WriteLine("OnNavigatedTo");

            //chartControlOne.ResetChartData();
           // chartControlOne.SaveButtonPressed += SaveButtonPressed;

            HeartBeatEngine.Instance.DeviceConnectionUpdated += Instance_DeviceConnectionUpdated;
            HeartBeatEngine.Instance.ValueChangeCompleted += Instance_ValueChangeCompleted;

            if (_UseSimlator)
            {
                HeartBeatEngine.Instance.StartSimulator(120, 80, 2, 90, true);
                System.Diagnostics.Debug.WriteLine("Simulator started.");
            }
            else {
                DeviceName.Text = HeartBeatEngine.Instance.SelectedDevice.Name;
                HeartBeatEngine.Instance.InitializeServiceAsync(HeartBeatEngine.Instance.SelectedDevice.Id, GattCharacteristicUuids.HeartRateMeasurement);
                System.Diagnostics.Debug.WriteLine("initialized.");
            }

            //SetWaitVisibility(false);


        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("OnNavigatedFrom");

            //chartControlOne.SaveButtonPressed -= SaveButtonPressed;
            HeartBeatEngine.Instance.DeviceConnectionUpdated -= Instance_DeviceConnectionUpdated;
            HeartBeatEngine.Instance.ValueChangeCompleted -= Instance_ValueChangeCompleted;
            HeartBeatEngine.Instance.Deinitialize();

            base.OnNavigatedFrom(e);
        }

        private async void SaveButtonPressed(ChartControlFull sender)
        {
           // if (sender == chartControlOne)
           // {
               // string dataToSave = chartControlOne.getDataString();
               // if (dataToSave == null)
                //{
                  //  ShowErrorDialog("Chart returned no data, please try again later.", "No data to save");
                  //  return;
                //}

                var savePicker = new Windows.Storage.Pickers.FileSavePicker();
                savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.DocumentsLibrary;
                // Dropdown of file types the user can save the file as
                savePicker.FileTypeChoices.Add("Plain Text", new List<string>() { ".txt" });
                // Default file name if the user does not type one in or select a file to replace
                savePicker.SuggestedFileName = "HeartBeat";

                StorageFile file = await savePicker.PickSaveFileAsync();
                if (file != null)
                {
                    CachedFileManager.DeferUpdates(file);
                  //  await FileIO.WriteTextAsync(file, dataToSave);

                    Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
                    if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                    {
                        ShowErrorDialog("File " + file.Name + " was saved.", "Save to file");
                    }
                    else
                    {
                        ShowErrorDialog("File " + file.Name + " couldn't be saved.", "Save to file");
                    }
                //}
            }
        }

        private async void Instance_DeviceConnectionUpdated(bool isConnected, string error)
        {
            // Serialize UI update to the the main UI thread.
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (error != null)
                {
                    ShowErrorDialog(error, "Connect error.");
                }

                if (isConnected)
                {
                    progressIndicator.Text = "Waiting for data...";
                   

                }
            });
        }

        public async void Instance_ValueChangeCompleted(HeartbeatMeasurement HeartbeatMeasurementValue)
        {
            System.Diagnostics.Debug.WriteLine("got heartbeat : " + HeartbeatMeasurementValue.HeartbeatValue);
            me = HeartbeatMeasurementValue.HeartbeatValue;

            // Serialize UI update to the the main UI thread.
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (progressGrid.Visibility == Visibility.Visible)
                {
                    SetWaitVisibility(false);             
                }

               // chartControlOne.AddChartData(HeartbeatMeasurementValue);
                myValues.Add(me);
                //avg = me/10;
                //System.Diagnostics.Debug.WriteLine("got avg : " + avg);
                //HrsValue.Text = pressureNorm.ToString();
                //library.Init(ref Display, ref Size, ref Colour);

            });

        }

        private void SetWaitVisibility(bool waitVisible)
        {
            progressGrid.Visibility = waitVisible ? Visibility.Visible : Visibility.Collapsed;
            valuesGrid.Visibility = waitVisible ? Visibility.Collapsed : Visibility.Visible;
            drawGrid.Visibility = waitVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        private async void ShowErrorDialog(string message, string title)
        {
            await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                var dialog = new MessageDialog(message, title);
                await dialog.ShowAsync();
            });
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //chartControlOne.ShowAllChartData();
        }

        #region ScribaMethods
        public static int smoothing(List<int> myValues)
        {
            int average;
            int sum = 0;

            avgValues2 = myValues.GetRange(myValues.Count - 3, 3);

            System.Diagnostics.Debug.WriteLine("Data: " + myValues.ToString());
            System.Diagnostics.Debug.WriteLine("Data Size: " + myValues.Count);
            System.Diagnostics.Debug.WriteLine("Subset Data: " + avgValues2.ToString());

            for (int j = 0; j < avgValues2.Count; j++)
            {
                sum += avgValues2[j];
            }

            average = sum / avgValues2.Count;
            System.Diagnostics.Debug.WriteLine("Average: " + average);

            for (int i = 0; i <= myValues.Count - 3; i++)
            {
                myValues.Remove(0);
            }

            myValues.Remove(myValues.Count-3);
            System.Diagnostics.Debug.WriteLine("Remove: " + myValues.ToString());

            myValues.Add(me);
            System.Diagnostics.Debug.WriteLine("Add: " + myValues.ToString());

            return average;
        }

        public static float SetBrushSize(float value)
        {
            fin = smoothing(myValues);

            float hrmHigh = 1000;
            float hrmLow = 0;

            if (fin > hrmHigh)
            {
                hrmHigh = fin;
            }

            if (fin < hrmLow)
            {
                hrmLow = fin;
            }

            float range = hrmHigh - hrmLow;

            float adjustment = 1000 / range;

            float newVal = fin * adjustment;

            return pressureNorm = (((1000 - newVal) / 1000)*value) + 1 ;
        }

        #endregion

        #region PointerEvents
        private void MyCanvas_PointerReleased(object sender, PointerRoutedEventArgs e)
        {

            if (e.Pointer.PointerId == _penID)
            {
                Windows.UI.Input.PointerPoint pt = e.GetCurrentPoint(MyCanvas);

                
                    // Pass the pointer information to the InkManager. 
                    _inkKhaled.ProcessPointerUp(pt);
                
            }

            else if (e.Pointer.PointerId == _touchID)
            {
                // Process touch input
                Windows.UI.Input.PointerPoint pt = e.GetCurrentPoint(MyCanvas);
                
                    // Pass the pointer information to the InkManager. 
                    _inkKhaled.ProcessPointerUp(pt);
            }

                _touchID = 0;
                _penID = 0;

                // Call an application-defined function to render the ink strokes.

                e.Handled = true;

            _undo.Push(l);
            _redo.Clear();
        }

        private void MyCanvas_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            SetBrushSize(value);

            if (e.Pointer.PointerId == _penID)
            {
                PointerPoint pt = e.GetCurrentPoint(MyCanvas);

                // Render a red line on the canvas as the pointer moves. 
                // Distance() is an application-defined function that tests
                // whether the pointer has moved far enough to justify 
                // drawing a new line.
                currentContactPt = pt.Position;
                x1 = _previousContactPt.X;
                y1 = _previousContactPt.Y;
                x2 = currentContactPt.X;
                y2 = currentContactPt.Y;

                if (Distance(x1, y1, x2, y2) > 0.0) // We need to developp this method now 
                {
                    l = new Line()
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2,
                        Fill = brush,
                        StrokeThickness = pressureNorm,
                        Stroke = brush,
                        //StrokeLineJoin = PenLineJoin.Round,
                        IsHoldingEnabled = true,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round
                    };


                _previousContactPt = currentContactPt;

                    // Draw the line on the canvas by adding the Line object as
                    // a child of the Canvas object.
                    MyCanvas.Children.Add(l);

                    // Pass the pointer information to the InkManager.
                    _inkKhaled.ProcessPointerUpdate(pt);
                }
            }

            else if (e.Pointer.PointerId == _touchID)
            {
                // Process touch input
                PointerPoint pt = e.GetCurrentPoint(MyCanvas);

                // Render a red line on the canvas as the pointer moves. 
                // Distance() is an application-defined function that tests
                // whether the pointer has moved far enough to justify 
                // drawing a new line.
                currentContactPt = pt.Position;
                x1 = _previousContactPt.X;
                y1 = _previousContactPt.Y;
                x2 = currentContactPt.X;
                y2 = currentContactPt.Y;


                if (Distance(x1, y1, x2, y2) > 0.0) // We need to developp this method now 
                {

                    l = new Line()
                    {
                        X1 = x1,
                        Y1 = y1,
                        X2 = x2,
                        Y2 = y2,
                        Fill = brush,
                        StrokeThickness = pressureNorm,
                        Stroke = brush,
                        //StrokeLineJoin = PenLineJoin.Round,
                        IsHoldingEnabled = true,
                        StrokeStartLineCap = PenLineCap.Round,
                        StrokeEndLineCap = PenLineCap.Round
                    };

                    _previousContactPt = currentContactPt;

                    // Draw the line on the canvas by adding the Line object as
                    // a child of the Canvas object.
                    MyCanvas.Children.Add(l);

                    // Pass the pointer information to the InkManager.
                    _inkKhaled.ProcessPointerUpdate(pt);
                }
            }

        }

        private double Distance(double x1, double y1, double x2, double y2)
        {
            double d = 0;
            d = Math.Sqrt(Math.Pow((x2 - x1), 2) + Math.Pow((y2 - y1), 2));
            return d;
        }

        private void MyCanvas_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            SetBrushSize(value);

            // Get information about the pointer location.
            PointerPoint pt = e.GetCurrentPoint(MyCanvas);
            _previousContactPt = pt.Position;

            // Accept input only from a pen or mouse with the left button pressed. 
            PointerDeviceType pointerDevType = e.Pointer.PointerDeviceType;
            if (pointerDevType == PointerDeviceType.Pen ||
                    pointerDevType == PointerDeviceType.Mouse &&
                    pt.Properties.IsLeftButtonPressed)
            {
      
                    // Pass the pointer information to the InkManager.
                    _inkKhaled.ProcessPointerDown(pt);
                    _penID = pt.PointerId;

                    e.Handled = true;
            }

            else if (pointerDevType == PointerDeviceType.Touch)
            { 
                    // Process touch input
                    // Pass the pointer information to the InkManager.
                    _inkKhaled.ProcessPointerDown(pt);
                    _penID = pt.PointerId;

                    e.Handled = true;
            }
        }

        #endregion

        #region Size and Colour Settings
        private void Size_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)Size.SelectedItem;
            string readValue = item.Tag.ToString();
            value = float.Parse(readValue);
        }

        private Color stringToColour(string value)
        {
            return Color.FromArgb(
            Byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber),
            Byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber),
            Byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber),
            Byte.Parse(value.Substring(6, 2), NumberStyles.HexNumber));
        }

        private void Colour_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)Colour.SelectedItem;
            string readValue = item.Tag.ToString();
            m_CurrentColour = stringToColour(readValue);
            brush = new SolidColorBrush(m_CurrentColour);
        }
        
        #endregion

        #region AppBar Buttons
        private void Undo(object sender, RoutedEventArgs e)
        {
            if (_undo.Any()) {
                line = MyCanvas.Children[MyCanvas.Children.Count - 1];
                var removeLine = _undo.Pop();
                MyCanvas.Children.Remove(line);
                _redo.Push(removeLine);
            }
        }

        private void Redo(object sender, RoutedEventArgs e)
        {
            if (_redo.Any())
            {
                var addLine = _redo.Pop();
                MyCanvas.Children.Add(line);
                _undo.Push(addLine);
            }
        }

        private void Clear(object sender, RoutedEventArgs e)
        {
            _undo.Clear();
            _redo.Clear();
            MyCanvas.Children.Clear();
        }

        private async void SaveMode(object sender, RoutedEventArgs e)
        {
           
            // Render to an image at the current system scale and retrieve pixel contents 
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap();
            await renderTargetBitmap.RenderAsync(MyCanvas);
            var pixelBuffer = await renderTargetBitmap.GetPixelsAsync();

            var savePicker = new FileSavePicker();
            savePicker.DefaultFileExtension = ".png";
            savePicker.FileTypeChoices.Add(".png", new List<string> { ".png" });
            savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
            savePicker.SuggestedFileName = "snapshot.png";

            // Prompt the user to select a file 
            var saveFile = await savePicker.PickSaveFileAsync();

            // Verify the user selected a file 
            if (saveFile == null)
                return;

            // Encode the image to the selected file on disk 
            using (var fileStream = await saveFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, fileStream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)renderTargetBitmap.PixelWidth,
                    (uint)renderTargetBitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    pixelBuffer.ToArray());

                await encoder.FlushAsync();
            }

            ShowErrorDialog("File saved.", "Save Image");


        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {

            Windows.Storage.Pickers.FileOpenPicker filepicker = new Windows.Storage.Pickers.FileOpenPicker();
            filepicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            filepicker.FileTypeFilter.Add(".jpg");
            filepicker.FileTypeFilter.Add(".png");
            filepicker.FileTypeFilter.Add(".bmp");
            filepicker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            Windows.Storage.StorageFile imageFile = await filepicker.PickSingleFileAsync();
            if (imageFile != null)
            {
                Windows.UI.Xaml.Media.Imaging.BitmapImage bitmap = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                Windows.Storage.Streams.IRandomAccessStream stream = await imageFile.OpenAsync(Windows.Storage.FileAccessMode.Read);
                Image newImage = new Image();
                bitmap.SetSource(stream);
                newImage.Source = bitmap;
                //newImage.Height = 250;
                newImage.Stretch = Stretch.UniformToFill;
                newImage.ManipulationMode = ManipulationModes.All;

                this.MyCanvas.Children.Add(newImage);
            }

        }       

        #endregion

    }
}
