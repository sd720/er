import ftplib
import os

FTP_HOST = "site61374.siteasp.net"
FTP_USER = "site61374"
FTP_PASS = "cX?8_4FdqZ=7"

ftp = ftplib.FTP(FTP_HOST)
ftp.login(FTP_USER, FTP_PASS)
ftp.cwd('wwwroot')

publish_dir = "publish"

def upload_dir(local_dir, remote_dir):
    try:
        ftp.mkd(remote_dir)
    except:
        pass
    ftp.cwd(remote_dir)
    for f in os.listdir(local_dir):
        local_path = os.path.join(local_dir, f)
        if os.path.isfile(local_path):
            print(f"Uploading {local_path} to {f}")
            with open(local_path, "rb") as file:
                ftp.storbinary(f"STOR {f}", file)
        elif os.path.isdir(local_path):
            upload_dir(local_path, f)
    ftp.cwd("..")

upload_dir(os.path.join(os.getcwd(), publish_dir), ".")
ftp.quit()
print("Deployment to MonsterASP.net via FTP complete!")
