!IF EXIST(Makefile.user)
!INCLUDE Makefile.user
!ENDIF

!IFNDEF TIMESTAMP_WEBSERVICE
TIMESTAMP_WEBSERVICE=/tr "http://sha256timestamp.ws.symantec.com/sha256/timestamp"
#TIMESTAMP_WEBSERVICE=/t "http://timestamp.globalsign.com/scripts/timestamp.dll"
#TIMESTAMP_WEBSERVICE=/t "http://timestamp.comodoca.com/authenticode"
#TIMESTAMP_WEBSERVICE=/t "http://timestamp.verisign.com/scripts/timestamp.dll"
!ENDIF

!IFNDEF ARCHDIR
!IF "$(_BUILDARCH)" == "x86"
ARCHDIR=i386
!ELSEIF "$(_BUILDARCH)" == "AMD64"
ARCHDIR=amd64
!ELSEIF "$(_BUILDARCH)" == "IA64"
ARCHDIR=ia64
!ELSE
!ERROR _BUILDARCH not defined. Please set environment variables for WDK 7.x or earlier build environments.
!ENDIF
!ENDIF

!IFNDEF SIGNTOOL
# To make signtool work with this Makefile, define:
# SIGNTOOL=signtool sign /a /v
# COMPANYNAME=Company name in your certificate
# COMPANYURL=http://www.ltr-data.se or your company website
# CERTPATH=Path to cross-sign certificate, or self-signed test cert
# To build without signing, leave following line defined instead
SIGNTOOL=@rem
!ENDIF

!IFNDEF CROSSCERT
!IFDEF CERTPATH
CROSSCERT=/ac "$(CERTPATH)"
!ENDIF
!ENDIF

BUILD_DEFAULT=-cegiw -nmake -i

INCLUDE=$(INCLUDE);$(MAKEDIR)\inc

README_TXT_FILES=LICENSE.md README.md

!IFNDEF DIST_DIR
DIST_DIR=$(MAKEDIR)\dist
!ENDIF

!IFNDEF UPLOAD_DIR
UPLOAD_DIR=$(MAKEDIR)\dist\setup
!ENDIF

STAMPINF_VERSION=$(FSCRYPTDISK_VERSION)

all: cli\$(ARCHDIR)\fscryptdisk.exe svc\$(ARCHDIR)\fscryptdsksvc.exe cpl\$(ARCHDIR)\fscryptdisk.cpl cplcore\$(ARCHDIR)\fscryptdisk.cpl sys\$(ARCHDIR)\fscryptdisk.sys awefsalloc\$(ARCHDIR)\awefsalloc.sys deviodrv\$(ARCHDIR)\deviodrv.sys deviotst\$(ARCHDIR)\deviotst.exe

clean:
	del /s *~ *.obj *.log *.wrn *.err *.mac *.o

publish: $(DIST_DIR) $(UPLOAD_DIR) $(DIST_DIR)\fscryptdiskinst.exe $(DIST_DIR)\fscryptdisk.zip $(DIST_DIR)\fscryptdisk_source.7z
	start $(UPLOAD_DIR)

$(DIST_DIR) $(UPLOAD_DIR):
	mkdir $@

$(DIST_DIR)\fscryptdiskinst.exe: $(DIST_DIR)\fscryptdisk.7z 7zS.sfx 7zSDcfg.txt
	copy /y /b 7zS.sfx + 7zSDcfg.txt + $(DIST_DIR)\fscryptdisk.7z $(DIST_DIR)\fscryptdiskinst.exe
	$(SIGNTOOL) /n "$(COMPANYNAME)" /d "FscryptDisk Virtual Disk Driver" /du "$(COMPANYURL)" $(CROSSCERT) $(TIMESTAMP_WEBSERVICE) $(DIST_DIR)\fscryptdiskinst.exe
	xcopy /d /y $(DIST_DIR)\fscryptdiskinst.exe $(UPLOAD_DIR)

$(DIST_DIR)\fscryptdisk_source.7z: $(DIST_DIR)\fscryptdisk.7z 7zSDcfg.txt $(README_TXT_FILES) runwaitw.exe install.cmd msgboxw.exe inc\fscryptdiskver.h devio\*.c devio\*.cpp devio\*.h devio\Makefile* uninstall_fscryptdisk.cmd 7zS.sfx *.sln *.props FscryptDiskNet\*.sln FscryptDiskNet\FscryptDiskNet\*.vb FscryptDiskNet\FscryptDiskNet\*.*proj FscryptDiskNet\DiscUtilsDevio\*.vb FscryptDiskNet\DiscUtilsDevio\*.*proj FscryptDiskNet\DevioNet\*.vb FscryptDiskNet\DevioNet\*.*proj Makefile devio\Makefile*
	del $(DIST_DIR)\fscryptdisk_source.7z
	7z a -r $(DIST_DIR)\fscryptdisk_source.7z -x!*~ -m0=PPMd 7zSDcfg.txt 7zS.sfx $(README_TXT_FILES) *.def *.src *.ico *.c *.h *.cpp *.hpp *.cxx *.hxx *.rc *.lib *.sln *.vb *.cs *.*proj *.snk *.resx *.resources *.myapp *.settings *.props Sources dirs fscryptdisk.inf runwaitw.exe install.cmd msgboxw.exe uninstall_fscryptdisk.cmd Makefile
	xcopy /d /y $(DIST_DIR)\fscryptdisk_source.7z $(UPLOAD_DIR)

$(DIST_DIR)\fscryptdisk.7z: $(README_TXT_FILES) fscryptdisk.inf runwaitw.exe install.cmd uninstall_fscryptdisk.cmd msgboxw.exe cli\i386\fscryptdisk.exe cpl\i386\fscryptdisk.cpl cplcore\i386\fscryptdisk.cpl svc\i386\fscryptdsksvc.exe sys\i386\fscryptdisk.sys awefsalloc\i386\awefsalloc.sys deviodrv\i386\deviodrv.sys cli\ia64\fscryptdisk.exe cpl\ia64\fscryptdisk.cpl cplcore\ia64\fscryptdisk.cpl svc\ia64\fscryptdsksvc.exe sys\ia64\fscryptdisk.sys awefsalloc\ia64\awefsalloc.sys deviodrv\ia64\deviodrv.sys cli\amd64\fscryptdisk.exe cpl\amd64\fscryptdisk.cpl cplcore\amd64\fscryptdisk.cpl svc\amd64\fscryptdsksvc.exe sys\amd64\fscryptdisk.sys awefsalloc\amd64\awefsalloc.sys deviodrv\amd64\deviodrv.sys cli\arm\fscryptdisk.exe cpl\arm\fscryptdisk.cpl cplcore\arm\fscryptdisk.cpl svc\arm\fscryptdsksvc.exe sys\arm\fscryptdisk.sys awefsalloc\arm\awefsalloc.sys deviodrv\arm\deviodrv.sys cli\arm64\fscryptdisk.exe cpl\arm64\fscryptdisk.cpl cplcore\arm64\fscryptdisk.cpl svc\arm64\fscryptdsksvc.exe sys\arm64\fscryptdisk.sys awefsalloc\arm64\awefsalloc.sys deviodrv\arm64\deviodrv.sys
	del $(DIST_DIR)\fscryptdisk.7z
	stampinf -f fscryptdisk.inf -a NTx86,NTia64,NTamd64,NTarm,NTarm64
	7z a $(DIST_DIR)\fscryptdisk.7z -m0=LZMA:a=2 $(README_TXT_FILES) fscryptdisk.inf runwaitw.exe install.cmd uninstall_fscryptdisk.cmd msgboxw.exe cli\i386\fscryptdisk.exe cpl\i386\fscryptdisk.cpl cplcore\i386\fscryptdisk.cpl svc\i386\fscryptdsksvc.exe sys\i386\fscryptdisk.sys awefsalloc\i386\awefsalloc.sys deviodrv\i386\deviodrv.sys cli\ia64\fscryptdisk.exe cpl\ia64\fscryptdisk.cpl cplcore\ia64\fscryptdisk.cpl svc\ia64\fscryptdsksvc.exe sys\ia64\fscryptdisk.sys awefsalloc\ia64\awefsalloc.sys deviodrv\ia64\deviodrv.sys cli\amd64\fscryptdisk.exe cpl\amd64\fscryptdisk.cpl cplcore\amd64\fscryptdisk.cpl svc\amd64\fscryptdsksvc.exe sys\amd64\fscryptdisk.sys awefsalloc\amd64\awefsalloc.sys deviodrv\amd64\deviodrv.sys cli\arm\fscryptdisk.exe cpl\arm\fscryptdisk.cpl cplcore\arm\fscryptdisk.cpl svc\arm\fscryptdsksvc.exe sys\arm\fscryptdisk.sys awefsalloc\arm\awefsalloc.sys deviodrv\arm\deviodrv.sys cli\arm64\fscryptdisk.exe cpl\arm64\fscryptdisk.cpl cplcore\arm64\fscryptdisk.cpl svc\arm64\fscryptdsksvc.exe sys\arm64\fscryptdisk.sys awefsalloc\arm64\awefsalloc.sys deviodrv\arm64\deviodrv.sys

$(DIST_DIR)\fscryptdisk.zip: $(DIST_DIR)\fscryptdisk.7z
	del $(DIST_DIR)\fscryptdisk.zip
	7z a $(DIST_DIR)\fscryptdisk.zip $(README_TXT_FILES) fscryptdisk.inf runwaitw.exe install.cmd uninstall_fscryptdisk.cmd msgboxw.exe cli\i386\fscryptdisk.exe cpl\i386\fscryptdisk.cpl cplcore\i386\fscryptdisk.cpl svc\i386\fscryptdsksvc.exe sys\i386\fscryptdisk.sys awefsalloc\i386\awefsalloc.sys deviodrv\i386\deviodrv.sys cli\ia64\fscryptdisk.exe cpl\ia64\fscryptdisk.cpl cplcore\ia64\fscryptdisk.cpl svc\ia64\fscryptdsksvc.exe sys\ia64\fscryptdisk.sys awefsalloc\ia64\awefsalloc.sys deviodrv\ia64\deviodrv.sys cli\amd64\fscryptdisk.exe cpl\amd64\fscryptdisk.cpl cplcore\amd64\fscryptdisk.cpl svc\amd64\fscryptdsksvc.exe sys\amd64\fscryptdisk.sys awefsalloc\amd64\awefsalloc.sys deviodrv\amd64\deviodrv.sys cli\arm\fscryptdisk.exe cpl\arm\fscryptdisk.cpl cplcore\arm\fscryptdisk.cpl svc\arm\fscryptdsksvc.exe sys\arm\fscryptdisk.sys awefsalloc\arm\awefsalloc.sys deviodrv\arm\deviodrv.sys cli\arm64\fscryptdisk.exe cpl\arm64\fscryptdisk.cpl cplcore\arm64\fscryptdisk.cpl svc\arm64\fscryptdsksvc.exe sys\arm64\fscryptdisk.sys awefsalloc\arm64\awefsalloc.sys deviodrv\arm64\deviodrv.sys
	xcopy /d /y $(DIST_DIR)\fscryptdisk.zip $(UPLOAD_DIR)

cli\$(ARCHDIR)\fscryptdisk.exe: cli\sources cli\*.c cli\*.rc inc\*.h cpl\$(ARCHDIR)\fscryptdisk.lib cplcore\$(ARCHDIR)\fscryptdisk.lib
	cd cli
	build
	cd $(MAKEDIR)
	editbin /nologo /subsystem:console,4.00 $@
	$(SIGNTOOL) /n "$(COMPANYNAME)" /d "FscryptDisk Virtual Disk Driver Command line tool" /du "$(COMPANYURL)" $(CROSSCERT) $(TIMESTAMP_WEBSERVICE) $@

cpl\$(ARCHDIR)\fscryptdisk.lib: cpl\$(ARCHDIR)\fscryptdisk.cpl

cpl\$(ARCHDIR)\fscryptdisk.cpl: cpl\sources cpl\*.c cpl\*.cpp cpl\*.rc cpl\*.src cpl\*.ico cpl\*.h cpl\*.manifest inc\*.h
	cd cpl
	build
	cd $(MAKEDIR)
	editbin /nologo /subsystem:windows,4.00 $@
	$(SIGNTOOL) /n "$(COMPANYNAME)" /d "FscryptDisk Virtual Disk Driver Control Panel Applet" /du "$(COMPANYURL)" $(CROSSCERT) $(TIMESTAMP_WEBSERVICE) $@

cplcore\$(ARCHDIR)\fscryptdisk.lib: cplcore\$(ARCHDIR)\fscryptdisk.cpl

cplcore\$(ARCHDIR)\fscryptdisk.cpl: cplcore\sources cplcore\*.c cpl\*.c cplcore\*.cpp cplcore\*.rc cpl\*.cpp cpl\*.rc cplcore\*.src cplcore\*.h cpl\*.h inc\*.h
	cd cplcore
	nmake refresh
	build
	cd $(MAKEDIR)
	editbin /nologo /subsystem:windows,4.00 $@
	$(SIGNTOOL) /n "$(COMPANYNAME)" /d "FscryptDisk Virtual Disk Driver Core API Library" /du "$(COMPANYURL)" $(CROSSCERT) $(TIMESTAMP_WEBSERVICE) $@

svc\$(ARCHDIR)\fscryptdsksvc.exe: svc\sources svc\*.cpp svc\*.rc inc\*.h inc\*.hpp
	cd svc
	build
	cd $(MAKEDIR)
	editbin /nologo /subsystem:console,4.00 $@
	$(SIGNTOOL) /n "$(COMPANYNAME)" /d "FscryptDisk Virtual Disk Driver Helper Service" /du "$(COMPANYURL)" $(CROSSCERT) $(TIMESTAMP_WEBSERVICE) $@

sys\$(ARCHDIR)\fscryptdisk.sys: sys\sources sys\*.cpp sys\*.h sys\*.rc inc\*.h
	cd sys
	build
	cd $(MAKEDIR)
	$(SIGNTOOL) /n "$(COMPANYNAME)" /d "FscryptDisk Virtual Disk Driver" /du "$(COMPANYURL)" $(CROSSCERT) $(TIMESTAMP_WEBSERVICE) $@

awefsalloc\$(ARCHDIR)\awefsalloc.sys: awefsalloc\sources awefsalloc\*.c awefsalloc\*.rc inc\*.h
	cd awefsalloc
	build
	cd $(MAKEDIR)
	$(SIGNTOOL) /n "$(COMPANYNAME)" /d "AWE Allocation Driver" /du "$(COMPANYURL)" $(CROSSCERT) $(TIMESTAMP_WEBSERVICE) $@

deviodrv\$(ARCHDIR)\deviodrv.sys: deviodrv\sources deviodrv\*.cpp deviodrv\*.rc deviodrv\*.h inc\*.h
	cd deviodrv
	build
	cd $(MAKEDIR)
	$(SIGNTOOL) /n "$(COMPANYNAME)" /d "DevIO Client Driver" /du "$(COMPANYURL)" $(CROSSCERT) $(TIMESTAMP_WEBSERVICE) $@

deviotst\$(ARCHDIR)\deviotst.exe: deviotst\sources deviotst\deviotst.cpp inc\*.h inc\*.hpp
	cd deviotst
	build
	cd $(MAKEDIR)
#	editbin /nologo /subsystem:console,4.00 $@
