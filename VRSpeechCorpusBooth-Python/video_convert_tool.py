import tkinter as tk
from tkinter import filedialog
from tkinter.ttk import Progressbar
import subprocess
import os


def execConversion(filepath,tag=""):
    filename = os.path.splitext(filepath)[0] 
    cmd = 'ffmpeg -i {} -filter:v scale=2880:-1 -c:v libvpx -crf 10 -b:v 10M -c:a libvorbis -auto-alt-ref 0 {}.webm'.format(filepath,filename+tag) #-crf 40 -b:v 900K  -crf 20  -profile 1
    print('Running this command:\n{}'.format(cmd))
    try:
        output = subprocess.check_output(cmd.split()).decode('utf-8')
    except subprocess.CalledProcessError:
        print('Python file did not exit successfully (likely crashed).')
    except OSError as e:
        print('Got an OS error not caused by permissions or a shebang problem; you\'ll have to take a look at it and see what the problem is. See below:')
        raise e  
    return output    


def execConversion2(filepath):
    filename = os.path.splitext(filepath)[0]
    cmd = 'ffmpeg -i {} -vn {}.wav'.format(filepath,filename)
    print('Running this command:\n{}'.format(cmd))
    try:
        output = subprocess.check_output(cmd.split()).decode('utf-8')
    except subprocess.CalledProcessError:
        print('Python file did not exit successfully (likely crashed).')
    except OSError as e:
        print('Got an OS error not caused by permissions or a shebang problem; you\'ll have to take a look at it and see what the problem is. See below:')
        raise e  
    return output       
    
def execute(root):
    progress = Progressbar(root,orient = tk.HORIZONTAL,length=120,mode='determinate')
    progress.pack(pady = 10)
    filePaths = filedialog.askopenfilenames()
    for idx,filepath in enumerate(filePaths):
        assert filepath.endswith('.mov') == True 
        execConversion(filepath,'lowRslu_0.75_')
        execConversion2(filepath)
        progress['value'] = 100 * (idx+1)/len(filePaths)
        root.update()
        
    root.destroy()
    
if __name__ == "__main__":
    root = tk.Tk()
    progress = Progressbar(root,orient = tk.HORIZONTAL,length=120,mode='determinate')
    progress.pack(pady = 10)
    filePaths = filedialog.askopenfilenames()
    for idx,filepath in enumerate(filePaths):
        # progress.update()
        assert filepath.endswith('.mov') == True 
        execConversion(filepath,"_lowRslu_0.5")
        execConversion2(filepath)
        progress['value'] = 100 * (idx+1)/len(filePaths)
        root.update()
        
    root.destroy()
    

