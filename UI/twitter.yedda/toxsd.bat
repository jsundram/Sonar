set xsdFile="C:\Documents and Settings\lee.keyser-allen\My Documents\Visual Studio 2005\Projects\WindowsApplication1\twitter.yedda\twitter_friends.xsd"
set outDirectory="C:\Documents and Settings\lee.keyser-allen\My Documents\Visual Studio 2005\Projects\WindowsApplication1\twitter.yedda\"
set xsdExeDir="C:\Program Files\Microsoft Visual Studio 8\SDK\v2.0\Bin"

rem set language="VB"
set language="CS"

set cwd=`pwd`
cd "%xsdExeDir%"
xsd.exe "%xsdFile%" /c /out:"%outDirectory%" /l:"%language%"
cd cwd
pause