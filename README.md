# Skype-WebView
Sample application using Skype (for Business) in a web-browser window to join a conference, without the Skype web-plugin, which is not supported on older versions of Windows.

# Usage
To access a specific Skype (for Business) infrastructure edit the configuration and set your domain's URL.
If user and password are specified, the app will perform auto login to the conference.

```xml
<setting name="ConferenceURL" serializeAs="String">
  <value>https://meet.yourdomain.com/yourconf/CONFERENCEID</value>
</setting>
<setting name="ProfileLanguage" serializeAs="String">
  <value>en-us</value>
</setting>
<setting name="ProfileUserName" serializeAs="String">
  <value />
</setting>
<setting name="ProfilePassword" serializeAs="String">
  <value />
</setting>
```

# Expected output
![Skype WebView Screenshot](https://raw.githubusercontent.com/cristipotlog/skype-webview/master/Screenshots/sample-login.png)

# Release notes
- May 17, 2016: Initial commit