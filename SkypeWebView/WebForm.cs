using System;
using System.Windows.Forms;

namespace SkypeWebView
{
    public partial class WebForm : Form
    {
        public WebForm()
        {
            InitializeComponent();
        }

        private void WebForm_Load( object sender, EventArgs e )
        {
            var uriBuilder = new UriBuilder( Properties.Settings.Default.ConferenceURL );
            uriBuilder.Query = string.Format( "sl={0}", 1 );

            this.webBrowser.Url = uriBuilder.Uri;
        }

        private void webBrowser_Navigated( object sender, WebBrowserNavigatedEventArgs e )
        {
            // wait for right page (there are multiple page redirects)
            if( e.Url.AbsolutePath != @"/lwa/WebPages/LwaClient.aspx" )
            {
                return;
            }

            // check if there are i-frames (we need one)
            var document = webBrowser.Document;
            if( document.Window.Frames.Count > 0 )
            {
                HtmlElement elem = document.GetElementById( "launchReachFrame" );
                if( elem != null )
                {
                    // get i-frame source url
                    string redirectUrl = elem.GetAttribute( "src" );
                    // add user locale (if configured)
                    if( string.IsNullOrEmpty( Properties.Settings.Default.ProfileLanguage ) == false )
                    {
                        redirectUrl = string.Format( @"{0}&reachLocale={1}", redirectUrl, Properties.Settings.Default.ProfileLanguage );
                    }
                    // reload i-frame in main browser window
                    document.Window.Navigate( redirectUrl );
                    return;
                }
            }
            else
            {
                // attach to window load
                document.Window.Load += OnFrameLoad;
            }
        }

        private void OnFrameLoad( object sender, HtmlElementEventArgs e )
        {
            var html = ( (HtmlWindow)sender ).Document;

            #region css layout optimize
            foreach( HtmlElement div in html.GetElementsByTagName( "div" ) )
            {
                if( div.GetAttribute( "className" ).StartsWith( "_DIV_PreAuthAreaView_9 _WelcomeArea_PreAuthAreaView_c" ) &&
                    div.Parent != null && div.Parent.GetAttribute( "className" ) == "_HeightMainArea_PreAuthAreaView_w" )
                {
                    div.Style = "width: 240px;";
                }
                else if( div.GetAttribute( "className" ).StartsWith( "_DIV_PreAuthAreaView_9 _PreAuthContainer_PreAuthAreaView_p" ) &&
                    div.Parent != null && div.Parent.GetAttribute( "className" ) == "_HeightMainArea_PreAuthAreaView_w" )
                {
                    div.Style = "left: 240px;";
                }
                else if( div.GetAttribute( "className" ).Contains( "_DIV_PreAuthAreaView_9 _innerStar_PreAuthAreaView_a" ) &&
                    div.Parent != null && div.Parent.GetAttribute( "className" ) == "_DIV_PreAuthAreaView_9" )
                {
                    div.Style = "padding-left: 80px; max-width: 420px;";
                }
                else if( div.GetAttribute( "className" ).StartsWith( "PreAuthAreaRegion" ) &&
                    div.Parent != null && div.Parent.GetAttribute( "className" ) == "PreAuthHolder" )
                {
                    div.Style = "min-height: 600px; min-width: 800px;";
                }
            }
            #endregion

            #region get buttons handles & hide unwanted
            foreach( HtmlElement button in html.GetElementsByTagName( "button" ) )
            {
                if( button.GetAttribute( "data-xid" ) == "Button" &&
                    button.OuterHtml.Contains( "_BigJoinButton_LoginControlView_" ) &&
                    button.Parent.OuterHtml.Contains( "class=\"_t29 " ) )
                {
                    button.SetAttribute( "id", "submitButton$TelemedicineLogin" );
                }
                else if( button.GetAttribute( "data-xid" ) == "Button" &&
                    button.OuterHtml.Contains( "_BigJoinButton_LoginControlView_" ) &&
                    button.Parent.GetAttribute( "data-id" ) == "anonymousLogin" )
                {
                    //button.SetAttribute( "id", "loginType$TelemedicineGuest" );
                    button.SetAttribute( "hidden", "true" ); // hide guest login button
                    button.Style = "visibility: hidden;"; // works on IE8/XP
                }
                else if( button.GetAttribute( "data-xid" ) == "Button" &&
                    button.OuterHtml.Contains( "_BigJoinButton_LoginControlView_" ) &&
                    button.Parent.GetAttribute( "data-id" ) == "formLogin" )
                {
                    //button.SetAttribute( "id", "loginType$TelemedicineForms" );
                    button.SetAttribute( "hidden", "true" ); // hide forms login button
                    button.Style = "visibility: hidden;"; // works on IE8/XP
                }
                else if( button.GetAttribute( "data-xid" ) == "Button" &&
                    button.OuterHtml.Contains( "_FooterCommon_FooterView_" ) &&
                    button.Parent.GetAttribute( "className" ) == "floatLeft" )
                {
                    button.SetAttribute( "hidden", "true" ); // hide privacy & cookies notice
                    button.Style = "visibility: hidden;"; // works on IE8/XP
                }
                else if( button.GetAttribute( "data-xid" ) == "Checkbox" &&
                    button.OuterHtml.Contains( "_checkbox_LoginControlView_" ) )
                {
                    //button.SetAttribute( "id", "checkBox$TelemedicineKeepUser" );
                    button.SetAttribute( "hidden", "true" ); // hide check box
                    button.Style = "visibility: hidden;"; // works on IE8/XP
                    button.Parent.Parent.SetAttribute( "hidden", "true" ); // hide check label
                    button.Parent.Parent.Style = "visibility: hidden;"; // works on IE8/XP
                }
            }
            #endregion

            #region auto login & remember user
            var js = string.Format( @"
// set default user name (simulate 'remember user')
Lync.Client.Controls.Common.JoinOptionsHelper.set_modeOfJoin(Lync.Client.Controls.Common.JoinOptionsHelper.forms);
Lync.Client.Controls.Common.JoinOptionsHelper.set_userName('TELEMED\\{0}');
Lync.Client.Controls.Common.JoinOptionsHelper.set_rememberMe(true);

// get viewmodel and set username & password
var my_start = Lync.Client.CoreUx.StartPage.$Z.get_decoratedViewModel(); // use Lync.Client.CoreUx.LayoutAwareViewModel
var my_login = my_start.get_loginControlViewModel(); // use Lync.Client.Controls.LoginControl.LoginControlViewModel
my_login.set_username('TELEMED\\{0}');
my_login.set_password('{1}');

// define auto login function
function telemedicineAutoLogin() {{
setTimeout( function() {{
    var my_submit = document.getElementById('submitButton$TelemedicineLogin');
    if( my_submit != null )
        my_submit.click(); // invoke click if button found
    else
        telemedicineAutoLogin(); // recurse until button found
}}, 1000);
}}", Properties.Settings.Default.ProfileUserName, Properties.Settings.Default.ProfilePassword );
            #endregion

            HtmlElement head = html.GetElementsByTagName( "head" )[0];
            HtmlElement script = html.CreateElement( "script" );
            script.SetAttribute( "type", "text/javascript" );
            script.SetAttribute( "text", js );
            head.AppendChild( script );

            html.InvokeScript( "telemedicineAutoLogin" );
        }
    }
}
