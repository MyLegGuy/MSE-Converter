//[[
To the extent possible under law, the person who associated CC0 with
_GameSpecific.lua has waived all copyright and related or neighboring rights
to _GameSpecific.lua.

You should have received a copy of the CC0 legalcode along with this
work. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
]]

OptionsSetTipExist(false)
OptionsEnableVoiceSetting();
OptionsSetTextMode(TEXTMODE_ADV);
OptionsLoadADVBox();
OptionsCanChangeBoxAlpha(false)
OptionsSetPositionSize(640,480);
OptionsSetIncludedFileExtensions(true)

// Constants for code
local FILETYPE_IMAGE=1;
local FILETYPE_SOUND=2;
// Constants that change game stuff
local BUSTFADEIN=300;
local MAXNAME = 0x08;
// filenames of the busts in the 3 locations
local curBusts = {nil,nil,nil};
local MAXBUSTS=3;
// filenames of the busts in the 3 locations that we are going to apply
local bustQueue = nil;
// which position each bustshot slot corresponds to
local slotPositions = {-160,0,160};
// last filename passed to the bustshow command. apply position command to this one.
local lastBustshotFilename=nil;
// only applies for the next message
local nextNameId=nil;

// For manual script mode
function main()
end
/////////////////////
// helper filenames
/////////////////////
function fixFilename(name, filetype)
	name = string.upper(name);
	if (filetype==FILETYPE_IMAGE) then
		return name .. ".PNG";
	elseif (filetype==FILETYPE_SOUND) then
		return name .. ".OGG";
	end
	return name;
end
function fixString(curString)
   local breakStart;
   local breakEnd;
   while(true) do
	  breakStart, breakEnd=string.find(curString,"\194\129\064\194\129\064");
	  if (breakStart==nil) then
		 break;
	  end
	  local segmentOneEnd=breakStart;
	  local segmentTwoStart=breakEnd+1;
	  // find the first non-space character before the start of the line break
	  for i=breakStart-1,1,-1 do
		 if (string.byte(curString,i)~=0x20) then
			segmentOneEnd=i;
			break;
		 end
	  end
	  // find the first non-space character after the line break
	  for i=breakEnd+1,string.len(curString) do
		 if (string.byte(curString,i)~=0x20) then
			segmentTwoStart=i;
			break;
		 end
	  end
	  // get rid of the line break. add a space between these two words though
	  curString = (string.sub(curString,1,segmentOneEnd) .. ' ' .. string.sub(curString,segmentTwoStart));
   end
   // replace special characters
   curString = string.gsub(curString,"\xc2\x81\x75",' "'); // left hook bracket
   curString = string.gsub(curString,"\xc2\x81\x76",'" '); // right hook bracket
   curString = string.gsub(curString,"\xc2\x81\x99",'☆'); // star
   curString = string.gsub(curString,"\xc2\x81\xf4",'♪'); // music note
   return curString;
end
// if it is not one of these, it is ignored
function isImportantBustID(id)
   return id>=0x04 and id~=0x0A;
end
function positionByteToSlot(_passedPositionByte)
	if (_passedPositionByte==0xCB) then // Far left
	   return 1;
	elseif (_passedPositionByte==0xC8) then // Middle
	   return 2;
	elseif (_passedPositionByte==0xCE) then // Right
	   return 3;
	else // Default, middle
		print("Unknown position byte. " .. _passedPositionByte)
		return 2;
	end
end
// LYPOS (int id, int pos, int _someNumber, int _someNumber+1)
function positionBust(args)
   if (isImportantBustID(args[1])) then
	  if (lastBustshotFilename==nil) then
		 print("warning: position command without bust");
		 return;
	  end
	  bustQueue[positionByteToSlot(args[2])]=lastBustshotFilename;
	  lastBustshotFilename=nil;
   end
end
// can be overridden 
function showDialogue(_words, _nameId)
   ClearMessage();
   OutputLine(nil,nil,_nameId,_words,Line_Normal);
end
/////////////////////
// Actual commands
/////////////////////
cmdTable = {};
function sonoBustshot(args)
   // if first arg is 0x0 -> second arg is 0
   // if first arg is 0xA -> DAMMY
   if (isImportantBustID(args[1])) then
	  if (bustQueue==nil) then
		 bustQueue = {nil,nil,nil};
	  end
	  if (lastBustshotFileame~=nil) then
		 print("warning: possibly lost a bust");
	  end
	  lastBustshotFilename=fixFilename(args[2],FILETYPE_IMAGE);
   end
end
function setSceneName(args)
   if (args~=nil) then
	  setVNDSVar(false,"curSceneName","=",'"' .. fixString(args[1]) .. '"');
   end
end
function showBackground(args)
   DisableWindow()
   // this will clear all the busts
   curBusts= {nil,nil,nil};
   DrawSceneWithMask(fixFilename(args[2],FILETYPE_IMAGE), "left", 0, 0, 300 );
end
function goodPlayBGM(args)
   PlayBGM(0,fixFilename(args[1],FILETYPE_SOUND),128,0);
end
function scriptShowDialogue(args)
   showDialogue(fixString(args[4]),nextNameId);
   nextNameId=nil;
end
function setTextColor(args)
   if (args[1]==0) then
	  // r = args[2]
   end
end
function goodPlayVoice(args)
   if (args[2]~="") then
	  PlayVoice(3,fixFilename(args[2],FILETYPE_SOUND),128);
   end
end
function goodPlaySE(args)
   PlaySE(4,fixFilename(args[2],FILETYPE_SOUND),128,64);
end
function setNextName(args)
   if (args[1]>=0x00 and args[1]<=MAXNAME) then
	  nextNameId=tonumber(args[1]);
   else
	  nextNameId=nil;
   end
end
/////////////////////
// gives the id and arg of the next command to execute
function preExecuteValidCommand(id, args)
   // if we are ready to apply the bust queue
   if (bustQueue~=nil and id~=0x66 and id~=0x67) then
	  local isNew = {};
	  local numChanges=0;
	  // first, instantly fade all bustshots we will not need anymore
	  for i=1,MAXBUSTS do
		 if (bustQueue[i]==nil) then
			FadeBustshot(i,FALSE,0,0,0,0,0,FALSE);
			curBusts[i]=nil;
		 else
			if (bustQueue[i]~=curBusts[i]) then
			   numChanges = numChanges+1;
			else
			   // keep this bust, but no fading because this bust is not changing
			end
		 end
	  end
	  // if there are new busts to draw
	  if (numChanges>0) then
		 // second, instantly draw the new bustshots under the old ones
		 for i=1,MAXBUSTS do
			if (bustQueue[i]~=nil and bustQueue[i]~=curBusts[i]) then
			   local fadeinTime=0;
			   // if this is a new bust
			   if (curBusts[i]==nil) then
				  fadeinTime=BUSTFADEIN;
				  numChanges=numChanges-1;
			   else
				  fadeinTime=0;
			   end
			   DrawBustshot(i+3,bustQueue[i],slotPositions[i],0,0,FALSE,0,0,0,0,0,0,0,0,fadeinTime,numChanges==0);
			end
		 end
		 // third, fade the old bustshots overtop. only wait for fade completion on the last one
		 for i=1,MAXBUSTS do
			if (curBusts[i]~=nil and bustQueue[i]~=curBusts[i]) then
			   FadeBustshot(i,FALSE,0,0,0,0,BUSTFADEIN,numChanges==1);
			   numChanges=numChanges-1;
			end
		 end
		 // final, the fading is now done. swap the bustshot slots.
		 for i=1,MAXBUSTS do
			if (bustQueue[i]~=nil and bustQueue[i]~=curBusts[i]) then
			   MoveBust(i+3,i);
			   // if the player pushed button to finish fade quickly, ensure all the busts finish fading
			   settleBust(i)
			end
		 end
	  end	 
	  curBusts=bustQueue;
	  bustQueue=nil;
   end
end
function c(id, args)
   if (cmdTable[id] ~= nil) then
	  preExecuteValidCommand(id,args)
	  cmdTable[id](args);
   end
end
// feel free to implement as many commands as you want!
cmdTable[0x3ED]=setSceneName;
cmdTable[0xC9]=goodPlayBGM;
cmdTable[0x64]=showBackground;
cmdTable[0x7DA]=scriptShowDialogue;
cmdTable[0x7D1]=setTextColor;
cmdTable[0x7D8]=goodPlayVoice;
cmdTable[0xD3]=goodPlaySE;
cmdTable[0x66]=sonoBustshot;
cmdTable[0x67]=positionBust;
cmdTable[0x7D7]=setNextName;
/////////////////////

oMenuVNDSSettings(0)
oMenuRestartBGM(0)
oMenuArtLocations(0)
oMenuScalingOption(0)
oMenuTextboxMode(0)
oMenuTextOverBG(0)
oMenuVNDSBustFade(0)
oMenuDebugButton(0)
dynamicAdvBoxHeight(0)

textOnlyOverBackground(1)

// setup adv name info
setADVNameSupport(2);
loadImageNameSheet("NAME.PNG");
// manual
defineImageName(0,27,3,90,32);
defineImageName(1,35,39,75,31);
defineImageName(2,37,73,71,33);
defineImageName(3,37,108,70,33);
defineImageName(4,38,142,69,33);
defineImageName(5,18,177,107,33);
defineImageName(6,25,212,94,33);
defineImageName(7,26,247,92,34);
defineImageName(8,43,285,58,30)
// (170/600)*480
advboxHeight(scalePixels(136,1))
// (32/600)*480
setADVNameImageHeight(scalePixels(25,1))
// (8/600)*480
setTextboxTopPad(scalePixels(6,1))
