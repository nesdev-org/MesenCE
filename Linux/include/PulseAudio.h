#pragma once

#include <cstdint>
#include <cstddef>
#include <sys/time.h>
#include <dlfcn.h>

extern "C"
{
	typedef uint64_t pa_usec_t;

	typedef struct pa_mainloop pa_mainloop;
	typedef struct pa_mainloop_api pa_mainloop_api;
	typedef struct pa_context pa_context;
	typedef struct pa_stream pa_stream;
	typedef struct pa_operation pa_operation;

	typedef enum pa_sample_format
	{
		PA_SAMPLE_U8,
		PA_SAMPLE_ALAW,
		PA_SAMPLE_ULAW,
		PA_SAMPLE_S16LE,
		PA_SAMPLE_S16BE,
		PA_SAMPLE_FLOAT32LE,
		PA_SAMPLE_FLOAT32BE,
		PA_SAMPLE_S32LE,
		PA_SAMPLE_S32BE,
		PA_SAMPLE_S24LE,
		PA_SAMPLE_S24BE,
		PA_SAMPLE_S24_32LE,
		PA_SAMPLE_S24_32BE,
		PA_SAMPLE_MAX,
		PA_SAMPLE_INVALID = -1
	} pa_sample_format_t;

	typedef enum pa_context_state
	{
		PA_CONTEXT_UNCONNECTED,
		PA_CONTEXT_CONNECTING,
		PA_CONTEXT_AUTHORIZING,
		PA_CONTEXT_SETTING_NAME,
		PA_CONTEXT_READY,
		PA_CONTEXT_FAILED,
		PA_CONTEXT_TERMINATED
	} pa_context_state_t;

	typedef enum pa_context_flags
	{
		PA_CONTEXT_NOFLAGS = 0x0000U,
		PA_CONTEXT_NOAUTOSPAWN = 0x0001U,
		PA_CONTEXT_NOFAIL = 0x0002U
	} pa_context_flags_t;

	typedef enum pa_seek_mode
	{
		PA_SEEK_RELATIVE = 0,
		PA_SEEK_ABSOLUTE = 1,
		PA_SEEK_RELATIVE_ON_READ = 2,
		PA_SEEK_RELATIVE_END = 3
	} pa_seek_mode_t;

	typedef enum pa_stream_flags
	{
		PA_STREAM_NOFLAGS = 0x0000U,
		PA_STREAM_START_CORKED = 0x0001U,
		PA_STREAM_INTERPOLATE_TIMING = 0x0002U,
		PA_STREAM_NOT_MONOTONIC = 0x0004U,
		PA_STREAM_AUTO_TIMING_UPDATE = 0x0008U,
		PA_STREAM_NO_REMAP_CHANNELS = 0x0010U,
		PA_STREAM_NO_REMIX_CHANNELS = 0x0020U,
		PA_STREAM_FIX_FORMAT = 0x0040U,
		PA_STREAM_FIX_RATE = 0x0080U,
		PA_STREAM_FIX_CHANNELS = 0x0100,
		PA_STREAM_DONT_MOVE = 0x0200U,
		PA_STREAM_VARIABLE_RATE = 0x0400U,
		PA_STREAM_PEAK_DETECT = 0x0800U,
		PA_STREAM_START_MUTED = 0x1000U,
		PA_STREAM_ADJUST_LATENCY = 0x2000U,
		PA_STREAM_EARLY_REQUESTS = 0x4000U,
		PA_STREAM_DONT_INHIBIT_AUTO_SUSPEND = 0x8000U,
		PA_STREAM_START_UNMUTED = 0x10000U,
		PA_STREAM_FAIL_ON_SUSPEND = 0x20000U,
		PA_STREAM_RELATIVE_VOLUME = 0x40000U,
		PA_STREAM_PASSTHROUGH = 0x80000U
	} pa_stream_flags_t;

	typedef enum pa_stream_state
	{
		PA_STREAM_UNCONNECTED,
		PA_STREAM_CREATING,
		PA_STREAM_READY,
		PA_STREAM_FAILED,
		PA_STREAM_TERMINATED
	} pa_stream_state_t;

	typedef enum pa_operation_state
	{
		PA_OPERATION_RUNNING,
		PA_OPERATION_DONE,
		PA_OPERATION_CANCELLED
	} pa_operation_state_t;

	struct pa_sample_spec
	{
		pa_sample_format_t format;
		uint32_t rate;
		uint8_t channels;
	};

	struct pa_buffer_attr
	{
		uint32_t maxlength;
		uint32_t tlength;
		uint32_t prebuf;
		uint32_t minreq;
		uint32_t fragsize;
	};

	struct pa_sink_info
	{
		const char* name;
		uint32_t index;
		const char* description;
		pa_sample_spec sample_spec;
	};

	struct pa_timing_info
	{
		struct timeval timestamp;
		int synchronized_clocks;
		pa_usec_t sink_usec;
		pa_usec_t source_usec;
		pa_usec_t transport_usec;
		int playing;
		int write_index_corrupt;
		int64_t write_index;
		int read_index_corrupt;
		int64_t read_index;
		pa_usec_t configured_sink_usec;
		pa_usec_t configured_source_usec;
		int64_t since_underrun;
	};

	typedef void (*pa_sink_info_cb_t)(pa_context* c, const pa_sink_info* i, int eol, void* userdata);
	typedef void (*pa_context_notify_cb_t)(pa_context* c, void* userdata);
	typedef void (*pa_stream_notify_cb_t)(pa_stream* p, void* userdata);
	typedef void (*pa_stream_success_cb_t)(pa_stream* s, int success, void* userdata);

	typedef const char* (*PFN_pa_strerror)(int);
	typedef int (*PFN_pa_context_errno)(const pa_context*);
	typedef pa_mainloop* (*PFN_pa_mainloop_new)();
	typedef void (*PFN_pa_mainloop_free)(pa_mainloop*);
	typedef pa_mainloop_api* (*PFN_pa_mainloop_get_api)(pa_mainloop*);
	typedef int (*PFN_pa_mainloop_iterate)(pa_mainloop*, int block, int* retval);
	typedef pa_context* (*PFN_pa_context_new)(pa_mainloop_api*, const char*);
	typedef void (*PFN_pa_context_unref)(pa_context*);
	typedef void (*PFN_pa_context_set_state_callback)(pa_context*, pa_context_notify_cb_t, void*);
	typedef int (*PFN_pa_context_connect)(pa_context*, const char*, pa_context_flags_t, const void*);
	typedef void (*PFN_pa_context_disconnect)(pa_context*);
	typedef pa_context_state_t (*PFN_pa_context_get_state)(pa_context*);
	typedef pa_operation* (*PFN_pa_context_get_sink_info_list)(pa_context*, pa_sink_info_cb_t, void*);
	typedef void (*PFN_pa_operation_unref)(pa_operation*);

	typedef pa_stream* (*PFN_pa_stream_new)(pa_context*, const char*, const pa_sample_spec*, const void*);
	typedef int (*PFN_pa_stream_connect_playback)(pa_stream*, const char*, const pa_buffer_attr*, pa_stream_flags_t, const void*, pa_stream*);
	typedef int (*PFN_pa_stream_begin_write)(pa_stream*, void**, size_t*);
	typedef int (*PFN_pa_stream_write)(pa_stream*, const void*, size_t, void (*)(void*), int64_t, pa_seek_mode_t);
	typedef void (*PFN_pa_stream_unref)(pa_stream*);
	typedef int (*PFN_pa_stream_disconnect)(pa_stream*);
	typedef pa_stream_state_t (*PFN_pa_stream_get_state)(const pa_stream*);
	typedef const pa_timing_info* (*PFN_pa_stream_get_timing_info)(pa_stream* s);
	typedef pa_operation* (*PFN_pa_stream_update_timing_info)(pa_stream* s, pa_stream_success_cb_t cb, void* userdata);
	typedef pa_operation_state_t (*PFN_pa_operation_get_state)(const pa_operation* o);
	typedef void (*PFN_pa_stream_set_underflow_callback)(pa_stream* p, pa_stream_notify_cb_t cb, void* userdata);
	typedef pa_operation* (*PFN_pa_stream_cork)(pa_stream* s, int b, pa_stream_success_cb_t cb, void* userdata);
	typedef pa_operation* (*PFN_pa_stream_flush)(pa_stream* s, pa_stream_success_cb_t cb, void* userdata);
	typedef int (*PFN_pa_stream_cancel_write)(pa_stream* s);

	inline PFN_pa_strerror ptr_pa_strerror = nullptr;
	inline PFN_pa_context_errno ptr_pa_context_errno = nullptr;
	inline PFN_pa_mainloop_new ptr_pa_mainloop_new = nullptr;
	inline PFN_pa_mainloop_free ptr_pa_mainloop_free = nullptr;
	inline PFN_pa_mainloop_get_api ptr_pa_mainloop_get_api = nullptr;
	inline PFN_pa_mainloop_iterate ptr_pa_mainloop_iterate = nullptr;
	inline PFN_pa_context_new ptr_pa_context_new = nullptr;
	inline PFN_pa_context_unref ptr_pa_context_unref = nullptr;
	inline PFN_pa_context_set_state_callback ptr_pa_context_set_state_callback = nullptr;
	inline PFN_pa_context_connect ptr_pa_context_connect = nullptr;
	inline PFN_pa_context_disconnect ptr_pa_context_disconnect = nullptr;
	inline PFN_pa_context_get_state ptr_pa_context_get_state = nullptr;
	inline PFN_pa_context_get_sink_info_list ptr_pa_context_get_sink_info_list = nullptr;
	inline PFN_pa_operation_unref ptr_pa_operation_unref = nullptr;

	inline PFN_pa_stream_new ptr_pa_stream_new = nullptr;
	inline PFN_pa_stream_connect_playback ptr_pa_stream_connect_playback = nullptr;
	inline PFN_pa_stream_begin_write ptr_pa_stream_begin_write = nullptr;
	inline PFN_pa_stream_write ptr_pa_stream_write = nullptr;
	inline PFN_pa_stream_unref ptr_pa_stream_unref = nullptr;
	inline PFN_pa_stream_disconnect ptr_pa_stream_disconnect = nullptr;
	inline PFN_pa_stream_get_state ptr_pa_stream_get_state = nullptr;
	inline PFN_pa_stream_get_timing_info ptr_pa_stream_get_timing_info = nullptr;
	inline PFN_pa_stream_update_timing_info ptr_pa_stream_update_timing_info = nullptr;
	inline PFN_pa_operation_get_state ptr_pa_operation_get_state = nullptr;
	inline PFN_pa_stream_set_underflow_callback ptr_pa_stream_set_underflow_callback = nullptr;
	inline PFN_pa_stream_cork ptr_pa_stream_cork = nullptr;
	inline PFN_pa_stream_flush ptr_pa_stream_flush = nullptr;
	inline PFN_pa_stream_cancel_write ptr_pa_stream_cancel_write = nullptr;

#define pa_strerror ptr_pa_strerror
#define pa_context_errno ptr_pa_context_errno
#define pa_mainloop_new ptr_pa_mainloop_new
#define pa_mainloop_free ptr_pa_mainloop_free
#define pa_mainloop_get_api ptr_pa_mainloop_get_api
#define pa_mainloop_iterate ptr_pa_mainloop_iterate
#define pa_context_new ptr_pa_context_new
#define pa_context_unref ptr_pa_context_unref
#define pa_context_set_state_callback ptr_pa_context_set_state_callback
#define pa_context_connect ptr_pa_context_connect
#define pa_context_disconnect ptr_pa_context_disconnect
#define pa_context_get_state ptr_pa_context_get_state
#define pa_context_get_sink_info_list ptr_pa_context_get_sink_info_list
#define pa_operation_unref ptr_pa_operation_unref

#define pa_stream_new ptr_pa_stream_new
#define pa_stream_connect_playback ptr_pa_stream_connect_playback
#define pa_stream_begin_write ptr_pa_stream_begin_write
#define pa_stream_write ptr_pa_stream_write
#define pa_stream_unref ptr_pa_stream_unref
#define pa_stream_disconnect ptr_pa_stream_disconnect
#define pa_stream_get_state ptr_pa_stream_get_state
#define pa_stream_get_timing_info ptr_pa_stream_get_timing_info
#define pa_stream_update_timing_info ptr_pa_stream_update_timing_info
#define pa_operation_get_state ptr_pa_operation_get_state
#define pa_stream_set_underflow_callback ptr_pa_stream_set_underflow_callback
#define pa_stream_cork ptr_pa_stream_cork
#define pa_stream_flush ptr_pa_stream_flush
#define pa_stream_cancel_write ptr_pa_stream_cancel_write

	inline bool LoadPulseAudio()
	{
		static void* handle = nullptr;
		if(handle) {
			return true;
		}

		handle = dlopen("libpulse.so.0", RTLD_LAZY | RTLD_LOCAL);
		if(!handle) {
			handle = dlopen("libpulse.so", RTLD_LAZY | RTLD_LOCAL);
		}
		if(!handle) {
			return false;
		}

#define LOAD_SYMBOL(type, name) \
	ptr_##name = (type)dlsym(handle, #name); \
	if (!ptr_##name) { return false; }

		LOAD_SYMBOL(PFN_pa_strerror, pa_strerror);
		LOAD_SYMBOL(PFN_pa_context_errno, pa_context_errno);
		LOAD_SYMBOL(PFN_pa_mainloop_new, pa_mainloop_new);
		LOAD_SYMBOL(PFN_pa_mainloop_free, pa_mainloop_free);
		LOAD_SYMBOL(PFN_pa_mainloop_get_api, pa_mainloop_get_api);
		LOAD_SYMBOL(PFN_pa_mainloop_iterate, pa_mainloop_iterate);
		LOAD_SYMBOL(PFN_pa_context_new, pa_context_new);
		LOAD_SYMBOL(PFN_pa_context_unref, pa_context_unref);
		LOAD_SYMBOL(PFN_pa_context_set_state_callback, pa_context_set_state_callback);
		LOAD_SYMBOL(PFN_pa_context_connect, pa_context_connect);
		LOAD_SYMBOL(PFN_pa_context_disconnect, pa_context_disconnect);
		LOAD_SYMBOL(PFN_pa_context_get_state, pa_context_get_state);
		LOAD_SYMBOL(PFN_pa_context_get_sink_info_list, pa_context_get_sink_info_list);
		LOAD_SYMBOL(PFN_pa_operation_unref, pa_operation_unref);
		LOAD_SYMBOL(PFN_pa_stream_new, pa_stream_new);
		LOAD_SYMBOL(PFN_pa_stream_connect_playback, pa_stream_connect_playback);
		LOAD_SYMBOL(PFN_pa_stream_begin_write, pa_stream_begin_write);
		LOAD_SYMBOL(PFN_pa_stream_write, pa_stream_write);
		LOAD_SYMBOL(PFN_pa_stream_unref, pa_stream_unref);
		LOAD_SYMBOL(PFN_pa_stream_disconnect, pa_stream_disconnect);
		LOAD_SYMBOL(PFN_pa_stream_get_state, pa_stream_get_state);
		LOAD_SYMBOL(PFN_pa_stream_get_timing_info, pa_stream_get_timing_info);
		LOAD_SYMBOL(PFN_pa_stream_update_timing_info, pa_stream_update_timing_info);
		LOAD_SYMBOL(PFN_pa_operation_get_state, pa_operation_get_state);
		LOAD_SYMBOL(PFN_pa_stream_set_underflow_callback, pa_stream_set_underflow_callback);
		LOAD_SYMBOL(PFN_pa_stream_cork, pa_stream_cork);
		LOAD_SYMBOL(PFN_pa_stream_flush, pa_stream_flush);
		LOAD_SYMBOL(PFN_pa_stream_cancel_write, pa_stream_cancel_write);

#undef LOAD_SYMBOL
		return true;
	}
}