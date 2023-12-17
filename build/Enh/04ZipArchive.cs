'From Squeak3.6 of ''6 October 2003'' [latest update: #5429] on 7 February 2004 at 4:57:47 pm'!!Archive methodsFor: 'testing' stamp: 'ar 12/2/2003 15:49'!hasMemberSuchThat: aBlock	^self members anySatisfy: aBlock! !!ArchiveViewer methodsFor: 'archive operations' stamp: 'ar 9/13/2002 01:14'!extractAll	| directory |	self canExtractAll ifFalse: [^ self].	directory _ FileList2 modalFolderSelector ifNil: [^ self].	archive extractAllTo: directory.! !!ZipArchive methodsFor: 'archive operations' stamp: 'ar 11/18/2003 23:53'!addDeflateString: aString as: aFileName	| mbr |	mbr := self addString: aString as: aFileName.	mbr desiredCompressionMethod: CompressionDeflated.	^mbr! !!ZipArchive methodsFor: 'archive operations' stamp: 'ar 9/13/2002 01:17'!extractAllTo: aDirectory	"Extract all elements to the given directory"	Utilities informUserDuring:[:bar|self extractAllTo: aDirectory informing: bar].! !!ZipArchive methodsFor: 'archive operations' stamp: 'ar 9/13/2002 01:42'!extractAllTo: aDirectory informing: bar	"Extract all elements to the given directory"	^self extractAllTo: aDirectory informing: bar overwrite: false! !!ZipArchive methodsFor: 'archive operations' stamp: 'ar 9/13/2002 01:42'!extractAllTo: aDirectory informing: bar overwrite: allOverwrite	"Extract all elements to the given directory"	| dir overwriteAll response |	overwriteAll := allOverwrite.	self members do:[:entry|		entry isDirectory ifTrue:[			bar ifNotNil:[bar value: 'Creating ', entry fileName].			dir := (entry fileName findTokens:'/') 					inject: aDirectory into:[:base :part| base directoryNamed: part].			dir assureExistence.		].	].	self members do:[:entry|		entry isDirectory ifFalse:[			bar ifNotNil:[bar value: 'Extracting ', entry fileName].			response := entry extractInDirectory: aDirectory overwrite: overwriteAll.			response == #retryWithOverwrite ifTrue:[				overwriteAll := true.				response := entry extractInDirectory: aDirectory overwrite: overwriteAll.			].			response == #abort ifTrue:[^self].			response == #failed ifTrue:[				(self confirm: 'Failed to extract ', entry fileName, '. Proceed?') ifFalse:[^self].			].		].	].! !!ZipArchiveMember methodsFor: 'accessing' stamp: 'ar 9/13/2002 01:26'!extractInDirectory: aDirectory overwrite: overwriteAll	"Extract this entry into the given directory. Answer #okay, #failed, #abort, or #retryWithOverwrite."	| path fileDir file index localName |	path := fileName findTokens:'/'.	localName := path last.	fileDir := path allButLast inject: aDirectory into:[:base :part| base directoryNamed: part].	file := [fileDir newFileNamed: localName] on: FileExistsException do:[:ex| ex return: nil].	file ifNil:[		overwriteAll ifFalse:[			[index := (PopUpMenu labelArray:{						'Yes, overwrite'. 						'No, don''t overwrite'. 						'Overwrite ALL files'.						'Cancel operation'					} lines: #(2)) startUpWithCaption: fileName, ' already exists. Overwrite?'.			index == nil] whileTrue.			index = 4 ifTrue:[^#abort].			index = 3 ifTrue:[^#retryWithOverwrite].			index = 2 ifTrue:[^#okay].		].		file := [fileDir forceNewFileNamed: localName] on: Error do:[:ex| ex return].		file ifNil:[^#failed].	].	self extractTo: file.	file close.	^#okay! !