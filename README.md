![linkedin](https://github.com/user-attachments/assets/4fa5221e-7ae5-4b6b-98a8-1c1e39b49afb)

# asv-ulog
ULog parser library

The provided [command line scripts](#scripts) are:
- `ulog_info`: display information from an ULog file.
- `ulog_messages`: display logged messages from an ULog file.
- `ulog_params`: extract parameters from an ULog file.

  ## Command Line Scripts
All scripts can be called by specifying the system path to .exe and support the `-h` flag for getting usage instructions.

The sections below show the usage syntax and sample output (from [src/Asv.Ulog.Tests/Resources/ulog_log_small.ulg](src/Asv.Ulog.Tests/Resources/ulog_log_small.ulg)): 

###  Show information about the ULog file (ulog_info)

Usage:
```bash
usage: ulog_info [-h] file.ulg

Display information from an ULog file

positional arguments:
  file.ulg       ULog input file

optional arguments:
  -h, --help     show this help message and exit
```

Example output:
```bash
PS C:\Users\1>Asv.Ulog.Shell.exe ulog_info ulog_log_small.ulg
Total info messages read: 14
Info messages:
ver_sw: 8583f1da30b63154d6ba0bc187d86135dfe33cf9
ver_sw_release: 17498624
ver_hw: CUBEPILOT_CUBEORANGE
sys_name: PX4
sys_os_name: NuttX
ver_sw_branch: v1.11.2_w_rc_sysid
sys_os_ver: ec20f2e6c5cc35b2b9bbe942dea55eabb81297b6
sys_os_ver_release: 134349055
sys_toolchain: GNU GCC
sys_toolchain_ver: 9.3.1 20200408 (release)
sys_mcu: STM32H7[4|5]xxx, rev. V
ver_data_format: 1
sys_uuid: 000600000000383638393239510d0035002d
time_ref_utc: 0
╔══════════════════════════════════════════╦═══════╗
║ Name (multi id, message size in bytes)   ║ Value ║
╠══════════════════════════════════════════╬═══════╣
║ actuator_armed (0, 17)                   ║ 0     ║
║ actuator_controls_0 (0, 22)              ║ 1     ║
║ actuator_controls_1 (0, 22)              ║ 2     ║
║ airspeed (0, 11)                         ║ 3     ║
║ airspeed_validated (0, 21)               ║ 4     ║
║ commander_state (0, 18)                  ║ 5     ║
║ cpuload (0, 10)                          ║ 6     ║
║ ekf_gps_drift (0, 16)                    ║ 7     ║
║ estimator_innovation_test_ratios (0, 35) ║ 8     ║
║ estimator_innovation_variances (0, 33)   ║ 9     ║
║ estimator_innovations (0, 24)            ║ 10    ║
║  ...                                     ║ ...   ║
║ vehicle_gps_position (0, 23)             ║ 61    ║
║ vehicle_imu (0, 14)                      ║ 62    ║
║ vehicle_imu (1, 14)                      ║ 63    ║
║ vehicle_imu (2, 14)                      ║ 64    ║
║ vehicle_imu_status (0, 21)               ║ 65    ║
║ vehicle_imu_status (1, 21)               ║ 66    ║
║ vehicle_imu_status (2, 21)               ║ 67    ║
║ ekf2_timestamps (0, 18)                  ║ 68    ║
║ vehicle_angular_acceleration (0, 31)     ║ 69    ║
║ logger_status (0, 16)                    ║ 70    ║
║ yaw_estimator_status (0, 23)             ║ 71    ║
╚══════════════════════════════════════════╩═══════╝
```

### Display logged messages from a ULog file (ulog_messages)

Usage:
```
usage: ulog_messages [-h] file.ulg

Display logged messages from an ULog file

positional arguments:
  file.ulg    ULog input file

optional arguments:
  -h, --help  show this help message and exit
```

Example output:
```
PS C:\Users\1>Asv.Ulog.Shell.exe ulog_messages ulog_log_small.ulg
00:00:22: Info [commander] Takeoff detected
00:00:23: Info [commander] Landing detected
00:00:25: Info [commander] Disarmed by landing
```

### Display parameters from a ULog file (ulog_params)

Usage:
```
usage: ulog_params [-h] file.ulg

Extract parameters from an ULog file

positional arguments:
  file.ulg              ULog input file

optional arguments:
  -h, --help            show this help message and exit
```

Example output (to console):
```
PS C:\Users\1>Asv.Ulog.Shell.exe ulog_params ulog_log_small.ulg
Total params read: 980
╔══════════════════╦═══════════════╗
║ Parameter        ║ Value         ║
╠══════════════════╬═══════════════╣
║ ASPD_BETA_GATE   ║ 1             ║
║ ASPD_BETA_NOISE  ║ 0,3           ║
║ ASPD_DO_CHECKS   ║ 0             ║
║ ASPD_FALLBACK    ║ 0             ║
║ ...              ║ ...           ║
║ WV_EN            ║ 1             ║
║ WV_GAIN          ║ 1             ║
║ WV_ROLL_MIN      ║ 1             ║
║ WV_YRATE_MAX     ║ 90            ║
╚══════════════════╩═══════════════╝
```
### Show statistics about the ULog file (ulog_statistic)

Usage:
```
usage: ulog_statistic [-h] file.ulg

Extract parameters from an ULog file

positional arguments:
  file.ulg              ULog input file

optional arguments:
  -h, --help            show this help message and exit
```

Example output (to console):
```
PS C:\Users\1>Asv.Ulog.Shell.exe ulog_statistic ulog_log_small.ulg
Total tokens read: 8575
╔════════════════════╦══════════════════╗
║ Tokens             ║ Number of tokens ║
╠════════════════════╬══════════════════╣
║ Unknown            ║ 0                ║
║ FileHeader         ║ 1                ║
║ FlagBits           ║ 1                ║
║ Format             ║ 82               ║
║ Information        ║ 14               ║
║ MultiInformation   ║ 116              ║
║ Parameter          ║ 980              ║
║ DefaultParameter   ║ 0                ║
║ Unsubscription     ║ 0                ║
║ Subscription       ║ 72               ║
║ LoggedData         ║ 7300             ║
║ LoggedString       ║ 0                ║
║ Synchronization    ║ 9                ║
║ TaggedLoggedString ║ 0                ║
║ Dropout            ║ 0                ║
╚════════════════════╩══════════════════╝
```


