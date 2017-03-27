# QvEpicTicketModule
QlikView web ticketing service for use with Epic Hyperspace implementations.

## Requirements 
* QlikView Server 11.2 or higher
    * QlikView Server must be configured to use IIS.  **This solution will not work with QlikView Webserver**.
    * DMS mode for authentication and qvw authorization.

* Epic Hyperspace 2015 (version 8.2)
    * Contact your Epic TSE for configuration instructions. 

* **[QlikViewEpic2015DLL](https://github.com/eapowertools/QlikViewEpic2015DLL)** downloaded, registered, and configured on the system running Epic Hyperspace client.

## Installation
1. Download a zip of the QvEpicTicketModule project to the QlikView server running QlikView web services.

2. Extract the files to a folder of your choosing.

3. Open the QvEpicTicketModule folder.  Copy epicwebticket.aspx and epicwebticket.aspx.cs files.

4. Paste the copied files in %QlikViewServerInstallPath%\Server\QlikViewClients\QlikViewAjax.  ___(%QlikViewServerInstallPath% default == c:\program files\qlikview)___

## Configuration
### Configuring QvEpicTicketModule
1. Inside the QvEpicTicketModule folder is a web.config file.  Open this file in your favorite text editor.

2. Observe the AppSettings section of the web.config file.
    ```
    <appSettings>
        <add key="QlikViewServerIPAddress" value="127.0.0.1" />
        <add key="QlikViewServerHostname" value="QlikView" />
        <add key="iv" value="0000000000000000" />
        <add key="sharedSecret" value="ABCDEFG123456789" />
    </appSettings>
    ```
    * The QlikViewServerIPAddress is the ip address used to contact the QlikView ticketing webservice to acquire a ticket for authentication.
    * The QlikViewServerHostname key takes a value representing the QlikView Server name.  This key/value pair is used by the ticket module to identify the server to connect to for redirecting requests.
    * The iv is a portion of the encryption component of the request coming from Epic.  It needs to be a 16 digit alphanumeric string.  The iv value must match the iv value set in the configuration file for the QlikViewEpic2015DLL.
    * The sharedSecret is the passphrase for the encryption component of the request coming from Epic.  It neesd to be a 16 digit alphanumeric string.  The sharedSecret value must match the key value set in the configuration file for the QlikViewEpic2015DLL.

3. Copy this section of the web.config file and paste it within the configuration element of the web.config file located at %QlikViewServerInstallPath%\Server\QlikViewClients\QlikViewAjax and save the file.  ___(%QlikViewServerInstallPath% default == c:\program files\qlikview)___

4. Now that this section exists in the web.config, configure it with the ip address of the server, the server name, and the values desired for the iv and sharedSecret.  Save the web.config.

### Configuring QlikView webservices to white list an ip address for ticketing
1. On the QlikView server running web services, Open a command prompt and type in ipconfig.  Make note of the IPV4 address of the QlikView Server. Example: 127.0.0.1.
1. On the QlikView server running web services, open the services snap-in and stop the QlikView Settings Service.
2. Navigate to %programdata%\qliktech\webserver.
3. Open the file named **config.xml**.
4. In the config.xml file, search for the section `<GetWebTicket url="/QvAjaxZfc/GetWebTicket.aspx" />`.
5. Change this section to the following:
    ```
    <GetWebTicket url="/QvAjaxZfc/GetWebTicket.aspx">
        <TrustedIP>127.0.0.1</TrustedIP> <!--where 127.0.0.1 is an example-->
    </GetWebTicket>
    ```
6. Save the config.xml file.
7. Return to the services snap-in and restart the QlikView Settings Service. 
