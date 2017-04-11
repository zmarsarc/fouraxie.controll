using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace GroundController {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        #region Constructor and disconstructor

        public MainWindow() {

            InitializeComponent();
            InitD3DImage();

            CompositionTarget.Rendering += new EventHandler(CompositionTarget_Rendering);

            SetMultiAdapterOptimization();
            SetupSurfaceResizing();

            SetupModelFileReader();
        }

        ~MainWindow() {
            Destroy();
        }

        #endregion

        #region D3D host

        DispatcherTimer _sizeTimer;
        DispatcherTimer _adapterTimer;
        TimeSpan _lastRender;

        /// <summary>
        ///
        /// Optional: Surface resizing
        ///
        /// The D3DImage is scaled when WPF renders it at a size 
        /// different from the natural size of the surface. If the
        /// D3DImage is scaled up significantly, image quality 
        /// degrades. 
        /// 
        /// To avoid this, you can either create a very large
        /// texture initially, or you can create new surfaces as
        /// the size changes. Below is a very simple example of
        /// how to do the latter.
        ///
        /// By creating a timer at Render priority, you are guaranteed
        /// that new surfaces are created while the element
        /// is still being arranged. A 200 ms interval gives
        /// a good balance between image quality and performance.
        /// You must be careful not to create new surfaces too 
        /// frequently. Frequently allocating a new surface may 
        /// fragment or exhaust video memory. This issue is more 
        /// significant on XDDM than it is on WDDM, because WDDM 
        /// can page out video memory.
        ///
        /// Another approach is deriving from the Image class, 
        /// participating in layout by overriding the ArrangeOverride method, and
        /// updating size in the overriden method. Performance will degrade
        /// if you resize too frequently.
        ///
        /// Blurry D3DImages can still occur due to subpixel 
        /// alignments. 
        /// </summary>
        private void SetupSurfaceResizing() {
            _sizeTimer = new DispatcherTimer(DispatcherPriority.Render);
            _sizeTimer.Tick += new EventHandler(SizeTimer_Tick);
            _sizeTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);
            _sizeTimer.Start();
        }

        private void InitD3DImage() {
            HRESULT.Check(SetSize(512, 512));
            HRESULT.Check(SetAlpha(false));
            HRESULT.Check(SetNumDesiredSamples(4));
        }

        /// <summary>
        /// 
        /// Optional: Surface resizing
        ///
        /// The D3DImage is scaled when WPF renders it at a size 
        /// different from the natural size of the surface. If the
        /// D3DImage is scaled up significantly, image quality 
        /// degrades. 
        /// 
        /// To avoid this, you can either create a very large
        /// texture initially, or you can create new surfaces as
        /// the size changes. Below is a very simple example of
        /// how to do the latter.
        ///
        /// By creating a timer at Render priority, you are guaranteed
        /// that new surfaces are created while the element
        /// is still being arranged. A 200 ms interval gives
        /// a good balance between image quality and performance.
        /// You must be careful not to create new surfaces too 
        /// frequently. Frequently allocating a new surface may 
        /// fragment or exhaust video memory. This issue is more 
        /// significant on XDDM than it is on WDDM, because WDDM 
        /// can page out video memory.
        ///
        /// Another approach is deriving from the Image class, 
        /// participating in layout by overriding the ArrangeOverride method, and
        /// updating size in the overriden method. Performance will degrade
        /// if you resize too frequently.
        ///
        /// Blurry D3DImages can still occur due to subpixel 
        /// alignments. 
        /// </summary>
        private void SetMultiAdapterOptimization() {
            _adapterTimer = new DispatcherTimer();
            _adapterTimer.Tick += new EventHandler(AdapterTimer_Tick);
            _adapterTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
            _adapterTimer.Start();
        }

        void AdapterTimer_Tick(object sender, EventArgs e) {
            POINT p = new POINT(imgelt.PointToScreen(new Point(0, 0)));

            HRESULT.Check(SetAdapter(p));
        }

        void SizeTimer_Tick(object sender, EventArgs e) {
            // The following code does not account for RenderTransforms.
            // To handle that case, you must transform up to the root and 
            // check the size there.

            // Given that the D3DImage is at 96.0 DPI, its Width and Height 
            // properties will always be integers. ActualWidth/Height 
            // may not be integers, so they are cast to integers. 
            uint actualWidth = (uint)imgelt.ActualWidth;
            uint actualHeight = (uint)imgelt.ActualHeight;
            if ((actualWidth > 0 && actualHeight > 0) &&
                (actualWidth != (uint)d3dimg.Width || actualHeight != (uint)d3dimg.Height)) {
                HRESULT.Check(SetSize(actualWidth, actualHeight));
            }
        }

        void CompositionTarget_Rendering(object sender, EventArgs e) {
            RenderingEventArgs args = (RenderingEventArgs)e;

            // It's possible for Rendering to call back twice in the same frame 
            // so only render when we haven't already rendered in this frame.
            if (d3dimg.IsFrontBufferAvailable && _lastRender != args.RenderingTime) {
                IntPtr pSurface = IntPtr.Zero;
                HRESULT.Check(GetBackBufferNoRef(out pSurface));
                if (pSurface != IntPtr.Zero) {

                    AddPoint(0f, 0f, 0f, 0xFFFFFFFF);
                    AddPoint(1f, 0f, 0f, 0xFFFFFFFF);
                    AddPoint(1f, 1f, 0f, 0xFFFFFFFF);
                    AddPoint(0f, 1f, 0f, 0xFFFFFFFF);
                    AddPoint(0f, 0f, 0f, 0xFFFFFFFF);
                    AddPoint(0f, 0f, 1f, 0xFFFFFFFF);
                    AddPoint(1f, 0f, 1f, 0xFFFFFFFF);
                    AddPoint(1f, 1f, 1f, 0xFFFFFFFF);
                    AddPoint(0f, 1f, 1f, 0xFFFFFFFF);
                    AddPoint(0f, 0f, 1f, 0xFFFFFFFF);

                    d3dimg.Lock();
                    // Repeatedly calling SetBackBuffer with the same IntPtr is 
                    // a no-op. There is no performance penalty.
                    d3dimg.SetBackBuffer(D3DResourceType.IDirect3DSurface9, pSurface);
                    HRESULT.Check(Render());
                    d3dimg.AddDirtyRect(new Int32Rect(0, 0, d3dimg.PixelWidth, d3dimg.PixelHeight));
                    d3dimg.Unlock();
                    _lastRender = args.RenderingTime;
                }
            }
        }

        #endregion

        #region Setup model file

        InputAdapter modFile;
        List<D3DObject> objects;

        private void SetupModelFileReader() {
            modFile = new InputAdapter();
            objects = modFile.Read("build.3ds");
        }

        #endregion

        #region Import methods
        // Import the methods exported by the unmanaged Direct3D content.

        [DllImport("D3DContent.dll")]
        static extern int GetBackBufferNoRef(out IntPtr pSurface);

        [DllImport("D3DContent.dll")]
        static extern int SetSize(uint width, uint height);

        [DllImport("D3DContent.dll")]
        static extern int SetAlpha(bool useAlpha);

        [DllImport("D3DContent.dll")]
        static extern int SetNumDesiredSamples(uint numSamples);

        [StructLayout(LayoutKind.Sequential)]
        struct POINT {
            public POINT(Point p) {
                x = (int)p.X;
                y = (int)p.Y;
            }

            public int x;
            public int y;
        }

        [DllImport("D3DContent.dll")]
        static extern int SetAdapter(POINT screenSpacePoint);

        [DllImport("D3DContent.dll")]
        static extern int AddPoint(float x, float y, float z, uint color);

        [DllImport("D3DContent.dll")]
        static extern int Render();

        [DllImport("D3DContent.dll")]
        static extern void Destroy();

        #endregion

        #region Camera control

        private Point lastPos;
        private CameraController camera = new CameraController(0.03f, 0.03f, 0.005f, 0.01f, 0.01f, 0f);

        private void imgelt_MouseMove(object sender, MouseEventArgs e) {

            var pos = e.GetPosition(this);

            float xPos = (float)(pos.X - lastPos.X);
            float yPos = (float)(pos.Y - lastPos.Y);
            lastPos = pos;

            if (e.LeftButton == MouseButtonState.Pressed) {
                camera.Move(-xPos, yPos, 0.0f);
            }
            else if (e.RightButton == MouseButtonState.Pressed) {
                camera.Rotate(yPos, xPos, 0f);
            }
            else {
                return;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e) {
            camera.MoveTo(0.0f, 0.0f, -5.0f);
            camera.LookAt(0.0f, 0.0f, 0.0f);
        }

        private void imgelt_MouseWheel(object sender, MouseWheelEventArgs e) {
            camera.Move(0.0f, 0.0f, e.Delta);
        }

        #endregion
    }

    public static class HRESULT {
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        public static void Check(int hr) {
            Marshal.ThrowExceptionForHR(hr);
        }
    }

    public class CameraController {

        private float xMoveSensitivity;
        private float yMoveSensitivity;
        private float zMoveSensitivity;
        private float xRotationSensitivity;
        private float yRotationSensitivity;
        private float zRotationSensitivity;

        public CameraController(
            float xMoveSens, float yMoveSens, float zMoveSens,
            float xRotationSens, float yRotationSens, float zRotationSens) {
            xMoveSensitivity = xMoveSens;
            yMoveSensitivity = yMoveSens;
            zMoveSensitivity = zMoveSens;
            xRotationSensitivity = xRotationSens;
            yRotationSensitivity = yRotationSens;
            zRotationSensitivity = zRotationSens;
        }

        public void MoveTo(float x, float y, float z) {
            HRESULT.Check(CameraMoveTo(x, y, z));
        }

        public void Move(float x, float y, float z) {
            HRESULT.Check(CameraMove(x * xMoveSensitivity, y * yMoveSensitivity, z * zMoveSensitivity));
        }

        public void LookAt(float x, float y, float z) {
            HRESULT.Check(CameraLookAt(x, y, z));
        }

        public void Rotate(float x, float y, float z) {
            HRESULT.Check(CameraRotate(x * xRotationSensitivity, y * yRotationSensitivity, z * zRotationSensitivity));
        }

        #region Import Methods

        [DllImport("D3DContent.dll")]
        static extern int CameraMoveTo(float x, float y, float z);

        [DllImport("D3DContent.dll")]
        static extern int CameraLookAt(float x, float y, float z);

        [DllImport("D3DContent.dll")]
        static extern int CameraMove(float x, float y, float z);

        [DllImport("D3DContent.dll")]
        static extern int CameraRotate(float x, float y, float z);

        #endregion

    }
}
