# Wave 6: Hardware I/O Communication - Manual Validation Checklist

## Overview
This checklist provides step-by-step instructions for validating Wave 6 hardware communication features with real physical hardware modules. Use this checklist for final validation before production deployment.

**Test Environment:**
- Physical AutoSteer module
- Physical Machine module (optional)
- Physical IMU module (optional)
- Computer with AgValoniaGPS installed
- USB-to-UDP adapters OR Bluetooth adapters OR CAN adapters

**Prerequisites:**
- All automated tests passing (58-76 tests)
- Hardware modules powered and functioning
- Transport adapters installed and configured

---

## Section 1: UDP Transport Validation

### 1.1 AutoSteer Module - UDP Connection

**Equipment:** AutoSteer module, UDP network connection

- [ ] **Step 1.1.1:** Power on AutoSteer module
- [ ] **Step 1.1.2:** Configure UDP settings in AgValoniaGPS (IP: 192.168.1.255, Port: 9999)
- [ ] **Step 1.1.3:** Start UDP transport for AutoSteer module
- [ ] **Step 1.1.4:** Verify hello packets received within 2 seconds
  - **Expected:** ModuleConnected event fires, module state = Ready
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 1.1.5:** Send steering command (10 mph, 5° angle, 100mm XTE, active)
- [ ] **Step 1.1.6:** Verify feedback received within 200ms
  - **Expected:** Actual wheel angle reported, switch states populated
  - **Actual:** Wheel angle = \_\_\_\_\_°, Switches = \_\_\_\_\_\_\_\_
- [ ] **Step 1.1.7:** Send 10 rapid steering commands (vary angle 0-20°)
- [ ] **Step 1.1.8:** Verify all commands processed without drops
  - **Expected:** 10 feedback messages received
  - **Actual:** \_\_\_\_\_ feedback messages received
- [ ] **Step 1.1.9:** Disconnect AutoSteer module power
- [ ] **Step 1.1.10:** Verify disconnection detected within 2 seconds
  - **Expected:** ModuleDisconnected event fires
  - **Actual:** Disconnected at \_\_\_\_\_\_ seconds

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

### 1.2 Machine Module - UDP Connection

**Equipment:** Machine module, UDP network connection

- [ ] **Step 1.2.1:** Power on Machine module
- [ ] **Step 1.2.2:** Start UDP transport for Machine module
- [ ] **Step 1.2.3:** Verify hello packets received within 2 seconds
  - **Expected:** Module state = Ready
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 1.2.4:** Send section states command (8 sections: on, on, off, on, off, on, on, off)
- [ ] **Step 1.2.5:** Verify section sensors match commanded state within 500ms
  - **Expected:** Section sensors = [1, 1, 0, 1, 0, 1, 1, 0]
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 1.2.6:** Toggle work switch on Machine module
- [ ] **Step 1.2.7:** Verify WorkSwitchChanged event fires
  - **Expected:** IsActive = true
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 1.2.8:** Send relay states command (relayLo: 0xFF, relayHi: 0x00)
- [ ] **Step 1.2.9:** Verify relays activate on hardware
  - **Expected:** Relays 0-7 ON, Relays 8-15 OFF
  - **Visual check:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

### 1.3 IMU Module - UDP Connection

**Equipment:** IMU module, UDP network connection

- [ ] **Step 1.3.1:** Power on IMU module
- [ ] **Step 1.3.2:** Start UDP transport for IMU module
- [ ] **Step 1.3.3:** Verify hello packets received within 2 seconds
  - **Expected:** Module state = Ready
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 1.3.4:** Wait for IMU data (PGN 219)
- [ ] **Step 1.3.5:** Verify data received at ~3Hz (300ms intervals)
  - **Expected:** 3-4 updates per second
  - **Actual:** \_\_\_\_\_ updates per second
- [ ] **Step 1.3.6:** Tilt IMU module (roll +10°)
- [ ] **Step 1.3.7:** Verify roll value updates within 500ms
  - **Expected:** Roll ≈ +10° ±2°
  - **Actual:** Roll = \_\_\_\_\_°
- [ ] **Step 1.3.8:** Send IMU config command (configFlags: 0x07)
- [ ] **Step 1.3.9:** Verify configuration acknowledged
  - **Expected:** No errors, IMU continues sending data
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

## Section 2: Bluetooth Transport Validation

### 2.1 AutoSteer Module - Bluetooth SPP

**Equipment:** AutoSteer module with Bluetooth capability, Bluetooth adapter

- [ ] **Step 2.1.1:** Pair AutoSteer Bluetooth module with computer
- [ ] **Step 2.1.2:** Configure Bluetooth SPP transport in AgValoniaGPS
- [ ] **Step 2.1.3:** Start Bluetooth transport for AutoSteer
- [ ] **Step 2.1.4:** Verify connection established within 2 seconds
  - **Expected:** Module state = Ready
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 2.1.5:** Send steering command
- [ ] **Step 2.1.6:** Verify feedback received (latency may be higher than UDP)
  - **Expected:** Feedback received within 500ms
  - **Actual:** Latency = \_\_\_\_\_ ms
- [ ] **Step 2.1.7:** Walk 10 meters away from Bluetooth module
- [ ] **Step 2.1.8:** Verify connection maintained or disconnection detected
  - **Expected:** Connection maintained OR disconnection detected within 2s
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 2.1.9:** Return to range and verify auto-reconnect (if supported)
  - **Expected:** Connection re-established automatically
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

### 2.2 Bluetooth BLE Mode (if supported)

**Equipment:** BLE-enabled module, BLE adapter

- [ ] **Step 2.2.1:** Configure Bluetooth BLE mode
- [ ] **Step 2.2.2:** Scan for BLE devices
- [ ] **Step 2.2.3:** Connect to BLE module
- [ ] **Step 2.2.4:** Verify data exchange over BLE GATT characteristics
  - **Expected:** Commands sent, feedback received
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

**Status:** ☐ Pass  ☐ Fail  ☐ N/A (BLE not supported)
**Notes:**

---

## Section 3: CAN Bus Transport Validation

### 3.1 CAN Bus Communication

**Equipment:** CAN-enabled module, PCAN-USB adapter or SocketCAN interface

- [ ] **Step 3.1.1:** Connect CAN adapter to computer
- [ ] **Step 3.1.2:** Verify adapter detected (check device path: /dev/pcanusb0 or similar)
  - **Adapter found:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 3.1.3:** Configure CAN transport (adapter path, baudrate: 250kbps)
- [ ] **Step 3.1.4:** Start CAN transport
- [ ] **Step 3.1.5:** Send PGN message via CAN
- [ ] **Step 3.1.6:** Verify message transmitted on CAN bus (use CAN analyzer)
  - **Expected:** CAN ID, data payload correct
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 3.1.7:** Inject CAN message from external device
- [ ] **Step 3.1.8:** Verify message received and parsed correctly
  - **Expected:** Data parsed correctly
  - **Actual:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

**Status:** ☐ Pass  ☐ Fail  ☐ N/A (CAN not supported)
**Notes:**

---

### 3.2 Hybrid Mode (CAN + UDP Simultaneously)

**Equipment:** IMU on CAN, AutoSteer on UDP

- [ ] **Step 3.2.1:** Start CAN transport for IMU module
- [ ] **Step 3.2.2:** Start UDP transport for AutoSteer module
- [ ] **Step 3.2.3:** Verify both modules connect successfully
  - **IMU state:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
  - **AutoSteer state:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 3.2.4:** Send commands to both modules simultaneously
- [ ] **Step 3.2.5:** Verify both modules respond without interference
  - **IMU data received:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
  - **AutoSteer feedback received:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

**Status:** ☐ Pass  ☐ Fail  ☐ N/A (Hybrid mode not supported)
**Notes:**

---

## Section 4: Radio Transport Validation

### 4.1 LoRa Radio Communication

**Equipment:** LoRa radio modules (RFM95 or similar), long-range test area

- [ ] **Step 4.1.1:** Configure LoRa radio transport (frequency, transmit power)
- [ ] **Step 4.1.2:** Pair radios (transmitter and receiver)
- [ ] **Step 4.1.3:** Start radio transport
- [ ] **Step 4.1.4:** Send test message
- [ ] **Step 4.1.5:** Verify message received at base station
  - **Expected:** Message received with RSSI > -100dBm
  - **Actual:** RSSI = \_\_\_\_\_ dBm
- [ ] **Step 4.1.6:** Perform range test (walk 0.5 mile / 800m away)
- [ ] **Step 4.1.7:** Verify connection maintained at range
  - **Expected:** Messages still received, RSSI degraded but functional
  - **Actual:** RSSI at 0.5mi = \_\_\_\_\_ dBm, Messages: ☐ Received ☐ Lost

**Status:** ☐ Pass  ☐ Fail  ☐ N/A (LoRa not supported)
**Notes:**

---

### 4.2 900MHz Spread Spectrum Radio

**Equipment:** XBee or RFD900 radio modules

- [ ] **Step 4.2.1:** Configure 900MHz radio (baudrate, transmit power)
- [ ] **Step 4.2.2:** Connect radios to computer and module
- [ ] **Step 4.2.3:** Start radio transport
- [ ] **Step 4.2.4:** Send commands and verify feedback
  - **Latency:** \_\_\_\_\_ ms
  - **Packet loss:** \_\_\_\_\_%
- [ ] **Step 4.2.5:** Test at 1 mile range (field test)
  - **Connection maintained:** ☐ Yes ☐ No
  - **RSSI at 1 mile:** \_\_\_\_\_ dBm

**Status:** ☐ Pass  ☐ Fail  ☐ N/A (900MHz not supported)
**Notes:**

---

## Section 5: Multi-Module Concurrent Operation

### 5.1 All Three Modules Active Simultaneously

**Equipment:** AutoSteer, Machine, and IMU modules (any transport combination)

- [ ] **Step 5.1.1:** Start all three modules on different or same transports
  - **AutoSteer transport:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
  - **Machine transport:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
  - **IMU transport:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 5.1.2:** Verify all modules reach Ready state within 5 seconds
  - **AutoSteer:** ☐ Ready
  - **Machine:** ☐ Ready
  - **IMU:** ☐ Ready
- [ ] **Step 5.1.3:** Send commands to all modules simultaneously for 2 minutes
- [ ] **Step 5.1.4:** Monitor for data corruption or interference
  - **Corrupted messages:** \_\_\_\_\_ (should be 0)
  - **Dropped messages:** \_\_\_\_\_ (should be <1%)
- [ ] **Step 5.1.5:** Verify no module disconnects unexpectedly
  - **Unexpected disconnects:** \_\_\_\_\_ (should be 0)

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

## Section 6: Transport Switching

### 6.1 Switch AutoSteer from UDP to Bluetooth

**Equipment:** AutoSteer module with both UDP and Bluetooth capability

- [ ] **Step 6.1.1:** Start AutoSteer on UDP, verify Ready state
- [ ] **Step 6.1.2:** Send steering command, verify feedback received
- [ ] **Step 6.1.3:** Stop UDP transport
- [ ] **Step 6.1.4:** Verify module enters Disconnected state within 2 seconds
- [ ] **Step 6.1.5:** Start Bluetooth transport
- [ ] **Step 6.1.6:** Verify module reconnects and reaches Ready state
  - **Reconnect time:** \_\_\_\_\_ seconds (should be <1.5s total)
- [ ] **Step 6.1.7:** Send steering command, verify feedback received on Bluetooth
  - **Feedback received:** ☐ Yes ☐ No

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

## Section 7: Performance and Reliability

### 7.1 Message Latency

**Equipment:** AutoSteer module, high-speed network analyzer (optional)

- [ ] **Step 7.1.1:** Send 100 steering commands with timestamps
- [ ] **Step 7.1.2:** Measure round-trip time (send → receive feedback)
- [ ] **Step 7.1.3:** Calculate average latency
  - **Average latency:** \_\_\_\_\_ ms (should be <10ms for UDP)
  - **95th percentile latency:** \_\_\_\_\_ ms
  - **Maximum latency:** \_\_\_\_\_ ms

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

### 7.2 CRC Validation with Real Hardware

**Equipment:** Any module, packet injection tool

- [ ] **Step 7.2.1:** Connect module and establish communication
- [ ] **Step 7.2.2:** Inject a valid message (should be processed)
- [ ] **Step 7.2.3:** Inject a message with corrupted CRC
- [ ] **Step 7.2.4:** Verify corrupted message is rejected
  - **Corrupted message rejected:** ☐ Yes ☐ No
- [ ] **Step 7.2.5:** Verify valid messages still processed after corruption
  - **Valid messages processed:** ☐ Yes ☐ No

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

### 7.3 Extended Operation (Stress Test)

**Equipment:** All available modules

- [ ] **Step 7.3.1:** Start all modules and establish communication
- [ ] **Step 7.3.2:** Run continuous operation for 30 minutes
  - **Start time:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
  - **End time:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 7.3.3:** Send commands continuously (1Hz rate)
- [ ] **Step 7.3.4:** Monitor for memory leaks (check application memory usage)
  - **Initial memory:** \_\_\_\_\_ MB
  - **Final memory:** \_\_\_\_\_ MB
  - **Memory growth:** \_\_\_\_\_ MB (should be <10MB)
- [ ] **Step 7.3.5:** Monitor for unexpected disconnections
  - **Disconnections:** \_\_\_\_\_ (should be 0)
- [ ] **Step 7.3.6:** Monitor for data corruption
  - **CRC errors:** \_\_\_\_\_ (should be 0 or very low)

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

## Section 8: Closed-Loop Integration with Wave 3/4

### 8.1 Steering Closed-Loop with Real AutoSteer Module

**Equipment:** AutoSteer module, test vehicle or test rig

- [ ] **Step 8.1.1:** Enable steering coordinator service
- [ ] **Step 8.1.2:** Configure guidance line (AB line or curve)
- [ ] **Step 8.1.3:** Start autonomous steering
- [ ] **Step 8.1.4:** Verify steering commands sent based on cross-track error
  - **Commands sent:** ☐ Yes ☐ No
- [ ] **Step 8.1.5:** Verify actual wheel angle feedback received
  - **Feedback received:** ☐ Yes ☐ No
  - **Actual angle matches commanded:** ☐ Within 5° ☐ Larger error
- [ ] **Step 8.1.6:** Manually induce cross-track error (steer off line)
- [ ] **Step 8.1.7:** Verify steering corrects back to line
  - **Correction time:** \_\_\_\_\_ seconds

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

### 8.2 Section Control Closed-Loop with Real Machine Module

**Equipment:** Machine module, implement with sections

- [ ] **Step 8.2.1:** Enable section control service
- [ ] **Step 8.2.2:** Configure sections (count, widths)
- [ ] **Step 8.2.3:** Activate work switch on Machine module
- [ ] **Step 8.2.4:** Verify sections turn on based on coverage map
  - **Sections activated:** ☐ Yes ☐ No
- [ ] **Step 8.2.5:** Verify section sensor feedback matches commanded state
  - **Feedback matches:** ☐ Yes ☐ Partial ☐ No
- [ ] **Step 8.2.6:** Deactivate work switch
- [ ] **Step 8.2.7:** Verify all sections turn off immediately
  - **All sections off:** ☐ Yes ☐ No

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

## Section 9: Edge Cases and Error Handling

### 9.1 Module Firmware Version Mismatch

**Equipment:** Module with older firmware (v1) and newer firmware (v2)

- [ ] **Step 9.1.1:** Connect v1 firmware module
- [ ] **Step 9.1.2:** Verify backward compatibility (v1 PGNs used)
  - **V1 module works:** ☐ Yes ☐ No
- [ ] **Step 9.1.3:** Connect v2 firmware module
- [ ] **Step 9.1.4:** Verify capability negotiation detects v2 support
  - **V2 capabilities detected:** ☐ Yes ☐ No
- [ ] **Step 9.1.5:** Verify v2 PGNs used with v2 module
  - **V2 PGNs used:** ☐ Yes ☐ No

**Status:** ☐ Pass  ☐ Fail  ☐ N/A (only one firmware version available)
**Notes:**

---

### 9.2 Power Loss Recovery

**Equipment:** Any module

- [ ] **Step 9.2.1:** Establish communication with module
- [ ] **Step 9.2.2:** Power off module abruptly (simulate power loss)
- [ ] **Step 9.2.3:** Verify disconnection detected within 2 seconds
  - **Disconnected:** ☐ Yes ☐ No
- [ ] **Step 9.2.4:** Power on module again
- [ ] **Step 9.2.5:** Verify automatic reconnection
  - **Reconnected:** ☐ Yes ☐ No
  - **Reconnect time:** \_\_\_\_\_ seconds

**Status:** ☐ Pass  ☐ Fail
**Notes:**

---

### 9.3 Network Congestion / Packet Loss

**Equipment:** Network with controllable packet loss (or use network simulator)

- [ ] **Step 9.3.1:** Establish communication under normal conditions
- [ ] **Step 9.3.2:** Introduce 5% packet loss on network
- [ ] **Step 9.3.3:** Verify communication continues (with possible delays)
  - **Communication maintained:** ☐ Yes ☐ No
- [ ] **Step 9.3.4:** Increase to 20% packet loss
- [ ] **Step 9.3.5:** Verify system handles degradation gracefully
  - **Graceful degradation:** ☐ Yes ☐ No (explain): \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
- [ ] **Step 9.3.6:** Remove packet loss
- [ ] **Step 9.3.7:** Verify communication returns to normal
  - **Recovered:** ☐ Yes ☐ No

**Status:** ☐ Pass  ☐ Fail  ☐ N/A (network simulator not available)
**Notes:**

---

## Section 10: Final Sign-Off

### 10.1 Overall Validation Summary

**Total Tests Executed:** \_\_\_\_\_
**Tests Passed:** \_\_\_\_\_
**Tests Failed:** \_\_\_\_\_
**Tests Not Applicable:** \_\_\_\_\_

**Critical Issues Identified:**
1. \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
2. \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
3. \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

**Minor Issues Identified:**
1. \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
2. \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

**Recommendations:**
\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

---

### 10.2 Approval

**Validated By:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
**Date:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
**Signature:** \_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

**Approved for Production:** ☐ Yes  ☐ No (requires rework)

**Additional Notes:**
\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_
\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_\_

---

## Appendix: Troubleshooting Guide

### Common Issues and Solutions

**Issue:** Module not detected
- **Solution:** Check power, verify network/BT connection, check IP address/port configuration

**Issue:** Hello packets received but no data
- **Solution:** Verify module firmware version, check data flow timeout settings

**Issue:** High latency (>50ms)
- **Solution:** Check network congestion, verify transport type (Bluetooth has higher latency), reduce message rate

**Issue:** CRC errors frequent
- **Solution:** Check cable quality, verify network stability, check for EMI interference

**Issue:** Module disconnects unexpectedly
- **Solution:** Check power supply stability, verify transport adapter functioning, check hello timeout settings

**Issue:** Bluetooth pairing fails
- **Solution:** Reset Bluetooth module, check adapter compatibility, verify Bluetooth enabled on computer

**Issue:** CAN bus communication fails
- **Solution:** Verify CAN adapter detected, check baudrate matches module, check termination resistors

---

**Document Version:** 1.0
**Last Updated:** 2025-10-19
**Spec Reference:** Wave 6 Hardware I/O & Communication
