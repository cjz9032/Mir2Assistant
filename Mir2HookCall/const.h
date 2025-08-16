#pragma once


// const.h
// #define USE_VERSION_CS  // 注释这行就用第二个版本

#ifdef USE_VERSION_CS
    #define MIR_BU_DAO_ADDR    0x000000
    #define MIR_BU_DAO_HOOK    false
    #define DELPHI_STRINGLIST_CLEAR_CALL 0x43E820
    #define FRMMAIN_ADDR    0x72BF90
    #define FRM_DLG_ADDR    0x72B56C
    #define TLoginScene_ADDR    0x661C44
    #define TLoginScene_OK_CALL    0x5C5E74

    #define DRAW_SCREEN_ADDR    0x72BF94
    #define MIR_LOGIN_ACT_HOOK    0x1C426D
    #define MIR_LOGIN_PWD_HOOK    0x1C4290
    // # 暂时没用
    #define MIR_G_SERVER_NAME    0x32B6CC
    #define MIR_SELECT_SERVER_CALL    0x22E878
    // # 暂时没用

    #define MIR_Dlg_Ok_Click    0x58F82C
    #define MIR_SELECT_CHR_SCENE_ADDR    0x72BFA0
    #define MIR_START_BTN_CALL 0x5C6AF8
    #define MIR_CANCEL_ITEM_MOVING_CALL 0x646AF0
    #define MIR_DBotLogoutClick_CALL 0x5A7C94
    #define MIR_ALLOW_GROUP_CALL 0x631BD4
    #define MIR_GROUP_MEMBER_ADDR 0x72F668
    #define MIR_GROUP_ONE_CALL 0x6322AC
    #define MIR_GROUP_TWO_CALL 0x631C80
    #define MIR_CHAT_CALL 0x62F2B4
    #define MIR_SendClientMessage_CALL 0x62E34C
    #define MIR_GRID_FOO_ADDR 0x72B47C
    #define MIR_DMenuBuyClick_CALL 0x5A2750
    #define MIC_G_NPC_ID 0x72F640
    #define MIR_GET_GoodsList_CALL 0x631154
    #define MIR_SendBuyItem_CALL 0x631690
    #define MIR_STORE_ITEM_CALL 0x630F9C
    #define MIR_BACK_STORE_ITEM_CALL 0x000000
    #define MIR_REPAIR_ITEM_CALL 0x630DAC
    #define MIR_SELL_ITEM_CALL 0x630CD0
    #define MIR_NPC_2ND_TALK_CALL 0x630394
    #define MIR_EAT_ITEM_CALL 0x629FE0
    #define MIR_DItemGridGridSelect_CALL 0x59E938
    #define MIR_DSWWeaponClick_CALL 0x5937A0
    #define MIR_TAKE_OFF_DIRECT_CALL 0x62FE24
    #define MIR_TAKE_ON_DIRECT_CALL 0x000000
    #define MIR_SPELL_DIRECT_CALL 0x62FA94
    #define MIR_BUTCH_DIRECT_CALL 0x63026C
    #define MIR_PICKUP_DIRECT_CALL 0x62FCB8
    #define MIR_HK2_ADDR 0x2347A3
    #define MIR_HK5_ADDR 0x234493
    #define MIR_HK_BIOS_ADDR 0x22EF5C

    #define MIR_SKP_DLG1_ADDR 0x239733
    #define MIR_SKP_DLG2_ADDR 0x23976D
    #define MIR_SKP_DLG3_ADDR 0x23958F
    #define MIR_SKP_DLG4_ADDR 0x235876
    #define MIR_SKP_DLG5_ADDR 0x239750
    #define MIR_SKP_DLG6_ADDR 0x2395EF
    #define MIR_SKP_DLG_END_ADDR 0x23BDE8
    #define MIR_HK_RUN_HP_ADDR 0x1D3397
    #define MIR_HK_EXIT_BATTLE_ADDR 0x227DA6
    #define MIR_HK_AUTO_GROUP_1_ADDR 0x238AAE
    #define MIR_HK_AUTO_GROUP_2_ADDR 0x238BD3
    #define MIR_HK_AUTO_AGREE_ADDR 0x226D6A
    #define MIR_DSServer1Click_CALL 0x5905C8
    
#else
    #define MIR_BU_DAO_ADDR    0x1DF76C
    #define MIR_BU_DAO_HOOK    true
    #define DELPHI_STRINGLIST_CLEAR_CALL 0x43e870
    #define FRMMAIN_ADDR    0x7524B4
    #define FRM_DLG_ADDR    0x74350C
    #define TLoginScene_ADDR    0x67A23C
    #define TLoginScene_OK_CALL    0x56C488

    #define DRAW_SCREEN_ADDR    0x679E18
    #define MIR_LOGIN_ACT_HOOK    0x16A881
    #define MIR_LOGIN_PWD_HOOK    0x16A8A4
    // # 暂时没用
    #define MIR_G_SERVER_NAME    0x3526C0
    #define MIR_SELECT_SERVER_CALL    0x242A48
    // # 暂时没用

    #define MIR_Dlg_Ok_Click    0x59B340
    #define MIR_SELECT_CHR_SCENE_ADDR    0x7524C4
    #define MIR_START_BTN_CALL 0x56D10C
    #define MIR_CANCEL_ITEM_MOVING_CALL 0x65EA88
    #define MIR_DBotLogoutClick_CALL 0x5B7530
    #define MIR_ALLOW_GROUP_CALL 0x645F44
    #define MIR_GROUP_MEMBER_ADDR 0x7563CC
    #define MIR_GROUP_ONE_CALL 0x646630
    #define MIR_GROUP_TWO_CALL 0x645FF4
    #define MIR_CHAT_CALL 0x6434A0
    #define MIR_SendClientMessage_CALL 0x642524
    #define MIR_GRID_FOO_ADDR 0x7432F4
    #define MIR_DMenuBuyClick_CALL 0x5B112C
    #define MIC_G_NPC_ID 0x7563A4
    #define MIR_GET_GoodsList_CALL 0x6454AC
    #define MIR_SendBuyItem_CALL 0x6459F4
    #define MIR_STORE_ITEM_CALL 0x6452EC
    #define MIR_BACK_STORE_ITEM_CALL 0x645B6C
    #define MIR_REPAIR_ITEM_CALL 0x6450F8
    #define MIR_SELL_ITEM_CALL 0x645018
    #define MIR_NPC_2ND_TALK_CALL 0x6446D0
    #define MIR_EAT_ITEM_CALL 0x63D914
    #define MIR_DItemGridGridSelect_CALL 0x5ABB7C
    #define MIR_DSWWeaponClick_CALL 0x59F718
    #define MIR_TAKE_OFF_DIRECT_CALL 0x6440F4
    #define MIR_TAKE_ON_DIRECT_CALL 0x64401C
    #define MIR_SPELL_DIRECT_CALL 0x643C88
    #define MIR_BUTCH_DIRECT_CALL 0x6445AC
    #define MIR_PICKUP_DIRECT_CALL 0x643F84
    #define MIR_HK2_ADDR 0x24ADB9
    #define MIR_HK5_ADDR 0x24AAA3
    #define MIR_HK_BIOS_ADDR 0x243140

    #define MIR_SKP_DLG1_ADDR 0x250317
    #define MIR_SKP_DLG2_ADDR 0x250351
    #define MIR_SKP_DLG3_ADDR 0x250160
    #define MIR_SKP_DLG4_ADDR 0x24BF87
    #define MIR_SKP_DLG5_ADDR 0x250334
    #define MIR_SKP_DLG6_ADDR 0x2501C2
    #define MIR_SKP_DLG7_ADDR 0x25005C
    #define MIR_SKP_DLG8_ADDR 0x24FFD8
    
    #define MIR_SKP_DLG_END_ADDR 0x253107
    #define MIR_HK_RUN_HP_ADDR 0x1DFE47
    #define MIR_HK_EXIT_BATTLE_ADDR 0x23AC9D
    #define MIR_HK_AUTO_GROUP_1_ADDR 0x24F5F4
    #define MIR_HK_AUTO_GROUP_2_ADDR 0x24F719
    // 已有补丁
    #define MIR_HK_AUTO_AGREE_ADDR 0x000000 
    #define MIR_DSServer1Click_CALL 0x59C0DC

    #define MIR_LEARN_SKIP_ADDR 0x23DC8C
    
#endif
