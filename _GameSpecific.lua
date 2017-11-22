
// Keep track of which slots have busts in them. Only if it is the first bust in that slot do we do a fadein
local usedBustSlots = {false,false,false};
local usedBustsFilenames = {nil,nil,nil,nil};

function main()
end

function ShowBackground(filename)
	usedBustSlots = {false,false,false};
	usedBustsFilenames = {nil,nil,nil,nil};
	DrawSceneWithMask( filename, "left", 0, 0, 300 );
end

function ShowDialogue(line, name)
	ClearMessage();
	OutputLine(NULL,"",NULL, line, Line_Normal);
end
PlayBGMOld = PlayBGM;
function PlayBGM(filename)
	//PlayBGMOld()
	PlayBGMOld( 2, filename, 128, 0 );
end
PlayVoiceOld = PlayVoice;
function PlayVoice(filename)
	PlayVoiceOld( 3, filename, 128 );
end

PlaySEOld = PlaySE;
function PlaySE(filename)
	PlaySEOld( 3, filename, 128, 64 );
end


local function positionByteToSlotAndPosition(_passedPositionByte)
	// Adding 3 to bustshot slot should always be safe.
	local _foundBustPosition;
	local _foundBustSlot;
	if (_passedPositionByte==0xCB) then // Far left
		_foundBustSlot=1;
		_foundBustPosition=-160;
	elseif (_passedPositionByte==0xC8) then // Middle
		_foundBustSlot=2;
		_foundBustPosition=0;
	elseif (_passedPositionByte==0xCE) then // Right
		_foundBustSlot=3;
		_foundBustPosition=160;
	else // Default, middle
		_foundBustSlot=2;
		_foundBustPosition=0;
		print("Unknown position byte. " .. _passedPositionByte)
	end
	return _foundBustSlot, _foundBustPosition;
end

function ShowBustForecast(_passedForecastArray)
	local _tempSavedSlots = {false,false,false,false};
	for i=1,#_passedForecastArray do
		_tempSavedSlots[positionByteToSlotAndPosition(_passedForecastArray[i])]=true;
	end
	for i=1,#usedBustSlots do
		if (_tempSavedSlots[i]~=true) then
			usedBustSlots[i]=false;
			usedBustsFilenames[i]=nil;
			FadeBustshot( i, FALSE, 0, 0, 0, 0, 0, FALSE );
		end
	end
end

function ShowBust(filename, _passedPositionByte)
	local _foundBustPosition;
	local _foundBustSlot;
	_foundBustSlot, _foundBustPosition = positionByteToSlotAndPosition(_passedPositionByte);
	// body
	//DrawBustshotWithFiltering( 2, filename, "m1", 1, 0, 0, FALSE, 0, 0, 0, 0, 0, 10, 300, TRUE );
	if (usedBustSlots[_foundBustSlot]==false) then
		DrawBustshot( _foundBustSlot, filename, _foundBustPosition, 0, 0, FALSE, 0, 0, 0, 0, 0, 0, 0, 0, 200, TRUE );
	else
		if (usedBustsFilenames[_foundBustSlot]==filename) then
			return;
		end
		DrawBustshot( _foundBustSlot+3, filename, _foundBustPosition, 0, 0, FALSE, 0, 0, 0, 0, 0, 0, 0, 0, 0, FALSE );
		FadeBustshot( _foundBustSlot, FALSE, 0, 0, 0, 0, 200, TRUE );
		MoveBust(_foundBustSlot+3,_foundBustSlot);
	end
	usedBustSlots[_foundBustSlot]=true;
	usedBustsFilenames[_foundBustSlot]=filename;
end
