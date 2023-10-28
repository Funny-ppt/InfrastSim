import urllib3
import json

class Simulator:
    def __init__(self, host, data=None):
        """
        给定基础地址、可选的模拟器数据，向服务器发起创建模拟器请求并返回绑定该模拟器的对象
        """
        self.host = host
        self.http = urllib3.PoolManager()
        self.id = self._create_simulator(data)

    def _request(self, method, endpoint, fields=None, headers=None):
        url = f"{self.host}{endpoint}"
        body = json.dumps(fields) if fields else None
        response = self.http.request(
            method, url, body=body, headers=headers
        )
        if response.status == 200:
            try:
                return json.loads(response.data.decode('utf-8'))
            except json.JSONDecodeError:
                return True
        return False

    def _create_simulator(self, data):
        if data:
            result = self._request('POST', '/simulator?newRandom=true', fields=data, headers={'Accept': 'application/json'})
        else:
            result = self._request('GET', '/simulator', headers={'Accept': 'application/json'})
        return result["id"]

    def get_data(self, detailed=False):
        """
        获取基建序列化结果，可以供反序列化加载
        """
        detailed_str = "?detailed=true" if detailed else "?detailed=false"
        return self._request('GET', f'/simulator/{self.id}{detailed_str}', headers={'Accept': 'application/json'})

    def get_mower_data(self):
        """
        基于序列化结果获取一些额外的数据，注意该内容和反序列化接口不完全兼容
        """
        return self._request('GET', f'/simulator/{self.id}/mowerdata', headers={'Accept': 'application/json'})
    
    def run(self, seconds = 60):
        """
        执行给定的时间，默认1分钟
        """

        return self._request('POST', f'/simulator/{self.id}/simulate?seconds={seconds}')

    def set_facility_state(self, facility: str, data):
        return self._request('POST', f'/simulator/{self.id}/{facility}', fields=data)
    
    def set_facility_level(self, facility: str, level: int):
        data = {
            'level': level
        }
        return self.set_facility_state(facility, data)
    
    def set_operators(self, facility: str, operators: list[str]):
        """
        设定入住设施的干员
        """
        data = {
            'operators': operators
        }
        return self.set_facility_state(facility, data)
    
    def force_replace(self, facility: str, operators: list[str]):
        """
        设定入住设施的干员，并强制重新排序
        """
        data = {
            'operators-force-replace': operators
        }
        return self.set_facility_state(facility, data)
    
    def set_product(self, facility: str, product: str):
        """
        设置制造站产物
        """
        data = {
            'product': product
        }
        return self.set_facility_state(facility, data)

    def set_strategy(self, facility: str, strategy: str):
        """
        设定贸易站策略
        """
        data = {
            'strategy': strategy
        }
        return self.set_facility_state(facility, data)

    def use_drones(self, facility: str, drones: int):
        """
        使用无人机
        """
        data = {
            'drone': drones
        }
        return self.set_facility_state(facility, data)
        
    def remove_all(self, facility: str):
        """
        移除设施内所有干员
        """
        return self._request('DELETE', f'/simulator/{self.id}/{facility}/operators')

    def remove_at(self, facility: str, idx: int):
        """
        移除实施内第idx个干员(不考虑index，仅根据已存在的序号，从1开始)
        """
        return self._request('DELETE', f'/simulator/{self.id}/{facility}/operators/{idx}')

    def collect(self, facility: str, idx: int=0):
        """
        收集某设施的产物，不填idx为全部，填了表示贸易站第idx个订单，从1开始计数
        """
        return self._request('GET', f'/simulator/{self.id}/{facility}/collect/{idx}')

    def collect_all(self):
        """
        收集贸易站内所有可收集的产物
        """
        return self._request('GET', f'/simulator/{self.id}/collectAll')

    def add_drones(self, amount: int):
        """
        添加amount数量的无人机
        """
        return self._request('GET', f'/simulator/{self.id}/sanity?amount={amount}')

