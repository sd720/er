import ftplib

ftp = ftplib.FTP("site61374.siteasp.net")
ftp.login("site61374", "cX?8_4FdqZ=7")
ftp.cwd('wwwroot')
for file in ['default.aspx', 'default.html', 'index.html', 'index.aspx', 'hostingstart.html']:
    try:
        ftp.delete(file)
        print("Deleted", file)
    except:
        pass
ftp.quit()
