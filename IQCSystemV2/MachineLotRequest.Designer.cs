namespace IQCSystemV2
{
    partial class MachineLotRequest
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MachineLotRequest));
            this.machineLotRequestWebView = new Microsoft.Web.WebView2.WinForms.WebView2();
            ((System.ComponentModel.ISupportInitialize)(this.machineLotRequestWebView)).BeginInit();
            this.SuspendLayout();
            // 
            // machineLotRequestWebView
            // 
            this.machineLotRequestWebView.AllowExternalDrop = true;
            this.machineLotRequestWebView.CreationProperties = null;
            this.machineLotRequestWebView.DefaultBackgroundColor = System.Drawing.Color.White;
            this.machineLotRequestWebView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.machineLotRequestWebView.Location = new System.Drawing.Point(0, 0);
            this.machineLotRequestWebView.Name = "machineLotRequestWebView";
            this.machineLotRequestWebView.Size = new System.Drawing.Size(800, 450);
            this.machineLotRequestWebView.Source = new System.Uri("http://apbiphiqcwb01:8080/iqcv2/operations/machine_lot_request_integrated", System.UriKind.Absolute);
            this.machineLotRequestWebView.TabIndex = 0;
            this.machineLotRequestWebView.ZoomFactor = 1D;
            this.machineLotRequestWebView.CoreWebView2InitializationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2InitializationCompletedEventArgs>(this.machineLotRequestWebView_CoreWebView2InitializationCompleted);
            this.machineLotRequestWebView.NavigationCompleted += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs>(this.machineLotRequestWebView_NavigationCompleted);
            this.machineLotRequestWebView.WebMessageReceived += new System.EventHandler<Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs>(this.machineLotRequestWebView_WebMessageReceived);
            // 
            // MachineLotRequest
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.machineLotRequestWebView);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MachineLotRequest";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Machine Lot Request";
            ((System.ComponentModel.ISupportInitialize)(this.machineLotRequestWebView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Microsoft.Web.WebView2.WinForms.WebView2 machineLotRequestWebView;
    }
}