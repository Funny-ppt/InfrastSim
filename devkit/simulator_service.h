#ifndef SIMULATOR_SERVICE_H
#define SIMULATOR_SERVICE_H

#include <stdint.h> // for int32_t

#ifdef __cplusplus
extern "C" {
#endif

// Function declarations
int32_t CreateSimulator(void);
int32_t CreateSimulatorWithData(const char *pJson, int newRandom);
int32_t DestroySimulator(int32_t id);
const char *GetData(int32_t id, int detailed);
void Simulate(int32_t id, int32_t seconds);
void SetFacilityState(int32_t id, int32_t facility, const char *pJson);
void SetUpgraded(int32_t id, const char *pJson);
void SetLevel(int32_t id, int32_t facility, int32_t level);
void SetStrategy(int32_t id, int32_t facility, int32_t strategy);
void SetProduct(int32_t id, int32_t facility, int32_t product);
void RemoveOperator(int32_t id, int32_t facility, int32_t idx);
void RemoveOperators(int32_t id, int32_t facility);
void CollectAll(int32_t id);
void Collect(int32_t id, int32_t facility, int32_t idx);
int32_t UseDrones(int32_t id, int32_t facility, int32_t amount);
void Sanity(int32_t id, int32_t amount);
const char *GetDataForMower(int32_t id);

// Enum declarations
typedef enum {
    CombatRecord_0,
    CombatRecord_1,
    CombatRecord_2,
    Gold,
    OriginFragment_0,
    OriginFragment_1,
} Product;

typedef enum {
    ControlCenter,
    Office,
    Reception,
    Training,
    Crafting,
    Dormitory_1,
    Dormitory_2,
    Dormitory_3,
    Dormitory_4,
    B101,
    B102,
    B103,
    B201,
    B202,
    B203,
    B301,
    B302,
    B303,
} Facility;

#ifdef __cplusplus
}
#endif

#endif // SIMULATOR_SERVICE_H
