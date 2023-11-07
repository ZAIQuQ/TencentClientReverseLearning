#pragma once


void init() __attribute__((constructor));

class Hook
{
public:
	const char * getPlatformABI();
	Hook();
	~Hook();
};

