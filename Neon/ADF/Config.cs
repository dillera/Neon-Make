using System;
using System.Collections.Generic;
using System.Text;

namespace ADF
{
    public enum Size { ATT_LEN = 3, CMD_LEN = 4, PAGENAME_LEN = 9, BANK_COUNT = 20, SHR_COUNT = 27, HWR_COUNT = 28, ATT_COUNT = 42 }

    public enum ATASCII { EOL = 155 }
    //Current Document Version
    public enum Version { MAJOR = 0, MINOR = 1 }
    public enum MEM { DEFAULT_LOWMEM = 0x4000, MAX_DLSIZE = 253, DEFAULT_HIGHMEM = 0x8000 }


    public enum Map
    {
        RTCLOK1 = 0x12 /* Real time clock high byte */,
        RTCLOK2 = 0x13 /* Real time clock middle byte */,
        RTCLOK3 = 0x14 /* Real time clock low byte */,
        SAVMSC = 0x58 /* The lowest address of the screen memory */,
        ATRACT = 0x4D /* Attract mode  */,
        COLRSH = 0x4F /* Attract mode color shift */,
        VDSLST = 0x200 /* The vector (16-bit) for NMI Display List Interrupts (DLI) */,
        SDMCTL = 0x22F /* Direct Memory Access (DMA) enable */,
        SDLSTL = 0x230 /* DLIST Address 16-bit 560,561 */,
        GPRIOR = 0x26F /* Priority selection registershadow for 53275 ($D01B)*/,
        STICK0 = 0x278 /* The value of joystick 0. STICK registers are shadow locations for PIA locations 54016 and 54017 ($D300$D301) */,
        STRIG0 = 0x284 /* Stick trigger 0 */,
        //Shadow Registers
        HOLD4 = 0x2BC  /* Spare byte - Temporary register used in the DRAW command only;     */,
        HOLD5 = 0x2BD  /* Spare byte - Temporary register used in the DRAW command only;  */,
        PCOLOR0 = 0x2C0 /* Color of player 0 and missile 0 */,
        PCOLOR1 = 0x2C1 /* Color of player 1 and missile 1 */,
        PCOLOR2 = 0x2C2 /* Color of player 2 and missile 2 */,
        PCOLOR3 = 0x2C3 /* Color of player 3 and missile 3 */,
        COLOR0 = 0x2C4 /* Color of playfield 0 */,
        COLOR1 = 0x2C5 /* Color of playfield 1 */,
        COLOR2 = 0x2C6 /* Color of playfield 2 */,
        COLOR3 = 0x2C7 /* Color of playfield 3 & Player 5 */,
        COLOR4 = 0x2C8 /* Color of playfield 4 */,
        HELPFG = 0x2DC  /* Register to hold the HELP key status; 17 = HELP, 81 = SHIFT+HELP, 145 = CTRL+HELP */,
        CHACT = 0x2F3 /* Character Control */,
        CHBAS = 0x2F4 /* Character Base */,
        CHAR = 0x2FC /* Last key pressed */,
        IOCB0 = 0x340 /* IOCB0 */,
        //Hardware Registers
        HPOSP0 = 0xD000 /* (W) Horizontal position of player 0  (R) Missile 0 to playfield collision */,
        HPOSP1 = 0xD001 /* (W) Horizontal position of player 1  (R) Missile 1 to playfield collision */,
        HPOSP2 = 0xD002 /* (W) Horizontal position of player 2  (R) Missile 2 to playfield collision */,
        HPOSP3 = 0xD003 /* (W) Horizontal position of player 3  (R) Missile 3 to playfield collision */,
        HPOSM0 = 0xD004 /* (W) Horizontal position of missile 0 (R) Player 0 to playfield collisions */,
        HPOSM1 = 0xD005 /* (W) Horizontal position of missile 1 (R) Player 1 to playfield collisions */,
        HPOSM2 = 0xD006 /* (W) Horizontal position of missile 2 (R) Player 2 to playfield collisions */,
        HPOSM3 = 0xD007 /* (W) Horizontal position of missile 3 (R) Player 3 to playfield collisions */,
        SIZEP0 = 0xD008 /* (W) Size of player 0 (R) Missile 0 to player collisions */,
        SIZEP1 = 0xD009 /* (W) Size of player 1 (R) Missile 1 to player collisions */,
        SIZEP2 = 0xD00A /* (W) Size of player 2 (R) Missile 2 to player collisions */,
        SIZEP3 = 0xD00B /* (W) Size of player 3 (R) Missile 3 to player collisions */,
        SIZEM = 0xD00C /* (W) Size for all missiles (R) Player 0 to player collisions */,
        GRAFP0 = 0xD00D /* (W) Graphics shape for player 0 (R) Player 1 to player collisions */,
        GRAFP1 = 0xD00E /* (W) Graphics shape for player 1 (R) Player 2 to player collisions */,
        GRAFP2 = 0xD00F /* (W) Graphics shape for player 2 (R) Player 3 to player collisions */,
        GRAFP3 = 0xD010 /* (W) Graphics shape for player 3 (R) Joystick trigger 0 (644) */,
        GRAFM = 0xD011 /* (W) Graphics for all missiles   (R) Joystick trigger 1 (645) */,
        COLPM0 = 0xD012 /* Color of player 0 and missile 0 */,
        COLPM1 = 0xD013 /* Color of player 1 and missile 1 */,
        COLPM2 = 0xD014 /* Color of player 2 and missile 2 */,
        COLPM3 = 0xD015 /* Color of player 3 and missile 3 */,
        COLPF0 = 0xD016 /* Color of playfield 0 */,
        COLPF1 = 0xD017 /* Color of playfield 1 */,
        COLPF2 = 0xD018 /* Color of playfield 2 */,
        COLPF3 = 0xD019 /* Color of playfield 3 & Player 5 */,
        COLPF4 = 0xD01A /* Color of playfield 4 */,
        PRIOR = 0xD01B /* (W) Priority selection register */,
        GRACTL = 0xD01D /* (W) Used with DMACTL (location 54272; $D400) to latch all triggers to turn on players and missiles */,
        HITCLR = 0xD01E /* (W) Clear collision registers */,
        CONSOL = 0xD01F /* (W) CONSOL */,
        AUDCTL = 0xD208 /* (W) Audio control */,
        SKCTL = 0xD20F /* (W) Audio control */,
        SKREST = 0xD20A /* (R) Random number generator */,
        DMACTL = 0xD400 /* (W) Direct Memory Access (DMA) control */,
        CHACTL = 0xD401 /* (W) (W) Character mode control */,
        HSCROL = 0xD404 /* (W) Horizontal scroll register */,
        VSCROL = 0xD405 /* (W) Vertical scroll register */,
        PMBASE = 0xD407 /* (W) MSB of the player/missile base address  */,
        CHBASE = 0xD409 /* (W) Character base address; */,
        WSYNC = 0xD40A  /* (W) Wait for horizontal synchronization */,
        NMIEN = 0xD40E  /* Non-maskable interrupt (NMI) enable 192 to enable the Display List Interrupts. */,
        PORTB = 0xD301  /* memory mgt control */,
        SETVBV = 0xE45C  /* SET Deferred VBI */
    }

}
