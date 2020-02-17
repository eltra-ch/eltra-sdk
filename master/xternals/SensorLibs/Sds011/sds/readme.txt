/*
void usage ()
{
    p_printf(YELLOW, (char *)

    "%s [options]  (version %s)\n\n"

    "\nSDS-011 Options: \n\n"

    "-m             get current working mode\n"
    "-p             get current working period\n"
    "-r             get current reporting mode\n"
    "-d             get Device ID\n"
    "-f             get firmware version\n"
    "-o             get data                    (default : NO data)\n"

    "\nSDS-011 setting: \n\n"

    "-M [ S / W  ]  Set working mode (sleep or work)\n"
    "-P [ 0 - 30 ]  Set working period (minutes)\n"
    "-R [ Q / R  ]  Set reporting mode (query or reporting)\n"
    "-D [ 0xaabb ]  Set new device ID\n"

    "\nProgram setting: \n\n"

    "-q x:y         get data in query mode x times (0 = endless), y seconds delay.\n"
    "-H #           set correction for humidity  (e.g. 33.5 for 33.5%)\n"
    "-u device      set new device               (default = %s)\n"
    "-b             set no color output          (default : color)\n"
    "-h             show help info\n"
    "-v             set verbose / debug info     (default : NOT set\n",
     progname,PROGVERSION, port);
}*/

/*
void parse_cmdline(int opt, char *option, struct settings *action)
{
    char *p = option;
    int i = 0;
    char buf[10];

    switch (opt) {

    case 'b':   // set NO color output
        NoColor = true;
        break;

    case 'd':   // get device-id
        action->g_devid = true;
        break;

    case 'f':   // get firmware
        action->g_firmware = true;
        break;

    case 'm':   // get current working mode  (sleep/work)
        action->g_working_mode = true;
        break;

    case 'o':   // get data continuous
        action->s_reporting_mode = REPORT_STREAM;
        action->g_data = true;
        SetDataDisplay(true);
        break;

    case 'p':   // get working period
        action->g_working_period = true;
        break;

    case 'r':   // get current reporting mode (report/ query)
        action->g_reporting_mode = true;
        break;

    case 'u':   // Set new device
        strncpy(port,option,sizeof(port));
        break;

    case 'v':   // set debug output
        PrmDebug = true;
        break;

    case 'q':   // Set query reads and interval
        i = 0;
        while (*p != ':'){

             buf[i++] = *p++;

             if (i > sizeof(buf)){
                p_printf(RED,"query read amount too long %s\n", option);
                return EXIT_FAILURE;
             }
        }
        buf[i] = 0x0;
        action->q_loop = (uint8_t)strtod(buf, NULL);
        p++; // skip :

        i = 0;
        while (*p != 0x0){

             buf[i++] = *p++;

             if (i > sizeof(buf)){
                p_printf(RED,"query delay amount too long %s\n", option);
                return EXIT_FAILURE;
             }
        }

        buf[i] = 0x0;
        action->q_delay = (uint8_t)strtod(buf, NULL);

        // set reporting mode to query
        action->s_reporting_mode = REPORT_QUERY;

        break;

    case 'D':   // Set new device ID (0xaabb)
        if (strlen(p) == 6) {
            if (*p++ == '0'){
                if (*p++ == 'x'){
                    buf[0] = *p++;
                    buf[1] = *p++;
                    buf[2]= 0x0;
                    action->newid[1] = (uint8_t)strtol(buf, NULL, 16);

                    buf[0] = *p++;
                    buf[1] = *p++;
                    action->newid[0] = (uint8_t)strtol(buf, NULL, 16);

                    action->s_devid = true;
                    break;
                }
            }
        }
        p_printf(RED,"Invalid Device Id %s\n", option);
        return EXIT_FAILURE;
        break;

    case 'M':   // Set working  mode
        if (*option == 's' || *option == 'S') action->s_working_mode = MODE_SLEEP;
        else if (*option == 'w'|| *option == 'W') action->s_working_mode = MODE_WORK;
        else {
            p_printf(RED,"invalid working mode %s [ s or w ]\n", option);
            return EXIT_FAILURE;
        }
        break;

    case 'P':   // set working period
        action->s_working_period  = (uint8_t)strtod(option, NULL);

        if (action->s_working_period  < 0 || action->s_working_period  > 30){
            p_printf(RED,"invalid working period %d minutes. [ 0 - 30 ]\n", action->s_working_period );
            return EXIT_FAILURE;
        }

        break;

    case 'H':   // set relative humidity correction
        if (Set_Humidity_Cor(strtod(option, NULL)))
        {
            p_printf(RED,"Invalid Humidity : %s [1 - 100%]\n",option );
            return EXIT_FAILURE;
        }
        break;

    case 'R':   // Set reporting  mode
        if (*option == 'r' || *option == 'R') action->s_reporting_mode = REPORT_STREAM;
        else if (*option == 'q' || *option == 'Q') action->s_reporting_mode = REPORT_QUERY;
        else {
            p_printf(RED,"invalid reporting mode %s [ r or q ]\n", option);
            return EXIT_FAILURE;
        }
        break;

    default:    
        usage();
        return EXIT_FAILURE;
    }
}
*/