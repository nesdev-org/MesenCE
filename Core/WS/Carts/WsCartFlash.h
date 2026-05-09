#pragma once
#include "pch.h"
#include "WS/WsTypes.h"
#include "WS/Carts/WsCart.h"
#include "Utilities/ISerializable.h"
#include "Shared/MemoryType.h"

class WsCartFlash : public WsCart
{
public:
	WsCartFlash();
	virtual ~WsCartFlash() {}
};
