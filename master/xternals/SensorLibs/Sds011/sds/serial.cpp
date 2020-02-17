/*
 * This part of this code is mainly based on the work from karl,
 * found on:  github https://github.com/karlchen86/SDS011
 */

#define __USE_MISC

#include <errno.h>
#include <fcntl.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include <stdbool.h>
#ifdef __arm__
#include <termios.h>
#include <unistd.h>
#include <sys/ioctl.h>
#include <linux/usbdevice_fs.h>
#endif

#include "serial.h"

#ifdef __arm__
struct termios tty_back;
#endif
bool restore = false;

int ConfigureSerial(int fd, int speed)
{
#ifdef __arm__
    struct termios tty;

    if (tcgetattr(fd, &tty_back) < 0) {
        perror("tcgetattr");
        return EXIT_FAILURE;
    }

    if (tcgetattr(fd, &tty) < 0) {
        perror("tcgetattr");
        return EXIT_FAILURE;
    }

    cfsetospeed(&tty, (speed_t)speed);
    cfsetispeed(&tty, (speed_t)speed);

    tty.c_cflag |= (CLOCAL | CREAD);    /* ignore modem controls */
    tty.c_cflag &= ~CSIZE;
    tty.c_cflag |= CS8;         /* 8-bit characters */
    tty.c_cflag &= ~PARENB;     /* no parity bit */
    tty.c_cflag &= ~CSTOPB;     /* only need 1 stop bit */
    tty.c_cflag &= ~CRTSCTS;    /* no hardware flowcontrol */

    /* setup for non-canonical mode */
    tty.c_iflag &= ~(IGNBRK | BRKINT | PARMRK | ISTRIP | INLCR | IGNCR | ICRNL | IXON);
    tty.c_lflag &= ~(ECHO | ECHONL | ICANON | ISIG | IEXTEN);
    tty.c_oflag &= ~OPOST;

    /* fetch bytes as they become available */
    tty.c_cc[VMIN] = 1;
    tty.c_cc[VTIME] = 1;

    if (tcsetattr(fd, TCSANOW, &tty) != 0) {
        perror("tcsetattr");
        return EXIT_FAILURE;
    }

    restore=true;
#endif
    return EXIT_SUCCESS;
}

int set_blocking(int fd, int mcount)
{
#ifdef __arm__
    struct termios tty;

    if (tcgetattr(fd, &tty) < 0) {
        perror("tcgetattr");
        return EXIT_FAILURE;
    }

    tty.c_cc[VMIN] = mcount ? 1 : 0;
    tty.c_cc[VTIME] = 5;        /* half second timer */

    if (tcsetattr(fd, TCSANOW, &tty) < 0) {
        perror("tcsetattr");
        return EXIT_FAILURE;
    }
#endif

    return EXIT_SUCCESS;
}

// paulvha : added to restore
int restore_ser(int fd)
{
#ifdef __arm__
    if (restore)
    {
        if (tcsetattr(fd, TCSANOW, &tty_back) < 0) {
            perror("reset tcsetattr");
            return EXIT_FAILURE;
        }
    }
#endif
    return EXIT_SUCCESS;
}


