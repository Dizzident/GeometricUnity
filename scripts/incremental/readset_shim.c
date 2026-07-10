/* LD_PRELOAD read-set capture shim.
 *
 * The binding design calls for strace -f -e trace=openat; this system has
 * no syscall tracer installed (no strace/ltrace/fatrace/bpftrace/perf), so
 * we intercept the libc open family instead. .NET's System.Native does its
 * file IO through libc open/openat, so this captures the managed read-set.
 *
 * Every open with read access (O_RDONLY or O_RDWR; fopen modes containing
 * 'r' or '+') is logged as an ABSOLUTE path, one per line, appended to the
 * file named by $READSET_LOG. Child processes inherit LD_PRELOAD and log to
 * the same file (O_APPEND keeps single write() calls atomic).
 *
 * Build: gcc -shared -fPIC -O2 -o readset_shim.so readset_shim.c -ldl
 */
#define _GNU_SOURCE
#include <dlfcn.h>
#include <fcntl.h>
#include <limits.h>
#include <stdarg.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <unistd.h>

static int (*real_open)(const char *, int, ...) = NULL;
static int (*real_open64)(const char *, int, ...) = NULL;
static int (*real_openat)(int, const char *, int, ...) = NULL;
static int (*real_openat64)(int, const char *, int, ...) = NULL;
static FILE *(*real_fopen)(const char *, const char *) = NULL;
static FILE *(*real_fopen64)(const char *, const char *) = NULL;

static const char *log_path(void) {
    static const char *p = NULL;
    static int resolved = 0;
    if (!resolved) {
        p = getenv("READSET_LOG");
        resolved = 1;
    }
    return p;
}

static void log_line(const char *abs) {
    const char *lp = log_path();
    if (!lp || !abs) return;
    if (strcmp(abs, lp) == 0) return; /* never log the log itself */
    if (!real_open) real_open = dlsym(RTLD_NEXT, "open");
    if (!real_open) return;
    int fd = real_open(lp, O_WRONLY | O_CREAT | O_APPEND, 0644);
    if (fd < 0) return;
    size_t len = strlen(abs);
    char buf[PATH_MAX + 2];
    if (len > PATH_MAX) len = PATH_MAX;
    memcpy(buf, abs, len);
    buf[len] = '\n';
    ssize_t ignored = write(fd, buf, len + 1);
    (void)ignored;
    close(fd);
}

/* Resolve a possibly-relative path (relative to cwd or to a dirfd) into an
 * absolute path buffer. Returns buf or NULL. */
static const char *resolve_path(int dirfd, const char *pathname, char *buf, size_t bufsz) {
    if (!pathname) return NULL;
    if (pathname[0] == '/') return pathname;
    char base[PATH_MAX];
    if (dirfd == AT_FDCWD) {
        if (!getcwd(base, sizeof(base))) return NULL;
    } else {
        char proc[64];
        snprintf(proc, sizeof(proc), "/proc/self/fd/%d", dirfd);
        ssize_t n = readlink(proc, base, sizeof(base) - 1);
        if (n <= 0) return NULL;
        base[n] = '\0';
    }
    if (snprintf(buf, bufsz, "%s/%s", base, pathname) >= (int)bufsz) return NULL;
    return buf;
}

static int wants_read(int flags) {
    int acc = flags & O_ACCMODE;
    return acc == O_RDONLY || acc == O_RDWR;
}

static void log_open(int dirfd, const char *pathname, int flags) {
    if (!wants_read(flags)) return;
    char buf[PATH_MAX + 1];
    const char *abs = resolve_path(dirfd, pathname, buf, sizeof(buf));
    if (abs) log_line(abs);
}

int open(const char *pathname, int flags, ...) {
    va_list ap;
    va_start(ap, flags);
    mode_t mode = va_arg(ap, mode_t);
    va_end(ap);
    if (!real_open) real_open = dlsym(RTLD_NEXT, "open");
    if (!real_open) return -1;
    log_open(AT_FDCWD, pathname, flags);
    return real_open(pathname, flags, mode);
}

int open64(const char *pathname, int flags, ...) {
    va_list ap;
    va_start(ap, flags);
    mode_t mode = va_arg(ap, mode_t);
    va_end(ap);
    if (!real_open64) real_open64 = dlsym(RTLD_NEXT, "open64");
    if (!real_open64) return -1;
    log_open(AT_FDCWD, pathname, flags);
    return real_open64(pathname, flags, mode);
}

int openat(int dirfd, const char *pathname, int flags, ...) {
    va_list ap;
    va_start(ap, flags);
    mode_t mode = va_arg(ap, mode_t);
    va_end(ap);
    if (!real_openat) real_openat = dlsym(RTLD_NEXT, "openat");
    if (!real_openat) return -1;
    log_open(dirfd, pathname, flags);
    return real_openat(dirfd, pathname, flags, mode);
}

int openat64(int dirfd, const char *pathname, int flags, ...) {
    va_list ap;
    va_start(ap, flags);
    mode_t mode = va_arg(ap, mode_t);
    va_end(ap);
    if (!real_openat64) real_openat64 = dlsym(RTLD_NEXT, "openat64");
    if (!real_openat64) return -1;
    log_open(dirfd, pathname, flags);
    return real_openat64(dirfd, pathname, flags, mode);
}

FILE *fopen(const char *pathname, const char *mode) {
    if (!real_fopen) real_fopen = dlsym(RTLD_NEXT, "fopen");
    if (!real_fopen) return NULL;
    if (mode && (strchr(mode, 'r') || strchr(mode, '+'))) {
        char buf[PATH_MAX + 1];
        const char *abs = resolve_path(AT_FDCWD, pathname, buf, sizeof(buf));
        if (abs) log_line(abs);
    }
    return real_fopen(pathname, mode);
}

FILE *fopen64(const char *pathname, const char *mode) {
    if (!real_fopen64) real_fopen64 = dlsym(RTLD_NEXT, "fopen64");
    if (!real_fopen64) return NULL;
    if (mode && (strchr(mode, 'r') || strchr(mode, '+'))) {
        char buf[PATH_MAX + 1];
        const char *abs = resolve_path(AT_FDCWD, pathname, buf, sizeof(buf));
        if (abs) log_line(abs);
    }
    return real_fopen64(pathname, mode);
}
