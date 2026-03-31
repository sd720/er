import ftplib
import os
import time

FTP_HOST = "site61374.siteasp.net"
FTP_USER = "site61374"
FTP_PASS = "cX?8_4FdqZ=7"

ftp = ftplib.FTP(FTP_HOST)
ftp.login(FTP_USER, FTP_PASS)
ftp.cwd('wwwroot')

print("Taking app offline...")
with open("app_offline.htm", "w") as f:
    f.write("<h1>Deploying updates... Please wait a few seconds.</h1>")
with open("app_offline.htm", "rb") as f:
    ftp.storbinary("STOR app_offline.htm", f)

time.sleep(3)

publish_dir = "publish"
files_to_upload = ["ItemProcessorApp.dll", "ItemProcessorApp.pdb", "ItemProcessorApp.exe"]

for f in files_to_upload:
    local_path = os.path.join(publish_dir, f)
    print(f"Uploading {local_path} to {f}")
    with open(local_path, "rb") as file:
        ftp.storbinary(f"STOR {f}", file)

print("Bringing app back online...")
try:
    ftp.delete('app_offline.htm')
except:
    pass

ftp.quit()
print("Quick deployment complete!")
