ADDR      = $0300
JIFFIES   = $06       ; The jiffies (60ths of a second) timer count (2 bytes)
START     = $FF

SID_BASE  = $D400               ; SID registers 

SID_V1FL  = SID_BASE+0          ; Voice 1
SID_V1FH  = SID_BASE+1
SID_V1PL  = SID_BASE+2
SID_V1PH  = SID_BASE+3
SID_V1CT  = SID_BASE+4
SID_V1AD  = SID_BASE+5
SID_V1SR  = SID_BASE+6

SID_V2FL  = SID_BASE+7          ; Voice 2
SID_V2FH  = SID_BASE+8
SID_V2PL  = SID_BASE+9
SID_V2PH  = SID_BASE+10
SID_V2CT  = SID_BASE+11
SID_V2AD  = SID_BASE+12
SID_V2SR  = SID_BASE+13

SID_V3FL  = SID_BASE+14         ; Voice 3
SID_V3FH  = SID_BASE+15
SID_V3PL  = SID_BASE+16
SID_V3PH  = SID_BASE+17
SID_V3CT  = SID_BASE+18
SID_V3AD  = SID_BASE+19
SID_V3SR  = SID_BASE+20

SID_FVOL  = SID_BASE+24         ; Volume Control

COUNT     = $FF

.WORD ADDR
.WORD (ADDR + LAST - MAIN - 1)

.LOGICAL    ADDR

MAIN        CLI
            LDX #24             ; Clear SID Registers
_CLEAR      STZ SID_BASE,X
            DEX
            BPL _CLEAR

SETUP_REGISTERS       
            LDA #$0F            ; Set main volume
            STA SID_FVOL

    ; Voice1: 
            LDA #$D6            ; Set frequency A4,7382, $1CD6          
            STA SID_V1FL
            LDA #$1C
            STA SID_V1FH
     
            LDA #$C0            ; Set Attack: 12 Decay: 0
            STA SID_V1AD
    
            LDA #$FC            ; Set Sustain: 15 Release: 12
            STA SID_V1SR

    ; Voice2:
            LDA #$0C            ; Set frequency C#5, 9228, $240C     
            STA SID_V2FL
            LDA #$24
            STA SID_V2FH

            LDA #$90            ; Set Attack: 9 Decay: 0
            STA SID_V2AD

            LDA #$F9            ; Set Sustain: 15 Release: 9
            STA SID_V2SR

    ; Voice3:
            LDA #$41            ; Set frequency E5, 11073, $2B41 
            STA SID_V3FL
            LDA #$2B
            STA SID_V3FH

            LDA #$20            ; Set Attack: 2 Decay: 0
            STA SID_V3AD

            LDA #$F4            ; Set Sustain: 15 Release: 4
            STA SID_V3SR


            LDA #$11            ; Begin playing (Triangle + Gate Bit)
            STA SID_V1CT
            STA SID_V2CT
            STA SID_V3CT

            LDA JIFFIES
            STA START
_WAIT       LDA JIFFIES         ; Wait 4 seconds
            SEC 
            SBC START
            CMP #240         
            BCC _WAIT
            
            LDA #$10            ; Stop playing
            STA SID_V1CT
            STA SID_V2CT
            STA SID_V3CT
            RTS

LAST                            ; End of the entire program

.ENDLOGICAL
            