#include "Coroutine.h"
#include "ThreadPool.h"
#include <iostream>
#include <chrono>
#include <map>

static ThreadPool io_threads(4);
static ThreadPool worker_threads(4);
static ThreadPool main_thread(1);

class Packet;
class AsyncTask
{
private :
    std::shared_ptr<Packet> packet;
    std::function<void()>   task;

public :
    AsyncTask(const std::shared_ptr<Packet>& packet, std::function<void()> task);

    constexpr bool await_ready() const noexcept;

    void await_suspend(std::coroutine_handle<> handle) const noexcept;

    void await_resume() const noexcept;
};

class Packet : public std::enable_shared_from_this<Packet>
{
    Coroutine<void> coroutine;
    std::string message;
public:
    Packet(const std::string& message)
        : message(message)
    {
        coroutine = Handler(message);
    }

    virtual Coroutine<void> Handler(const std::string& message)
    {
        std::cout << __FUNCTION__ << "_" << __LINE__ << " " << std::this_thread::get_id() << std::endl;

        co_await AsyncTask(shared_from_this(), []() {
            std::cout << __FUNCTION__ << "_" << __LINE__ << " " << std::this_thread::get_id() << std::endl;
            std::this_thread::sleep_for(std::chrono::milliseconds(1000));
            });

        std::cout << __FUNCTION__ << "_" << __LINE__ << " " << std::this_thread::get_id() << std::endl;
    }

    bool Resume()
    {
        return coroutine.resume();
    }
};

int main()
{
    for (int i = 0; i < 4; i++)
    {
        io_threads.PostTask([]() {
            while (true)
            {
                std::string message;
                std::cin >> message;
                std::shared_ptr<Packet> packet = std::make_shared<Packet>(message);
                main_thread.PostTask([packet]() {
                    packet->Resume();
                   });
            }
        });
    }

    main_thread.Join();
}

AsyncTask::AsyncTask(const std::shared_ptr<Packet>& packet, std::function<void()> task)
    : packet(packet)
    , task(task)
{
}

void AsyncTask::await_suspend(std::coroutine_handle<> handle) const noexcept
{
    auto packet = this->packet;
    auto task = this->task;
    worker_threads.PostTask([packet, task]() {

        task();

        main_thread.PostTask([packet]() {
            packet->Resume();
        });
    });
}

constexpr bool AsyncTask::await_ready() const noexcept
{
    return false;
}

void AsyncTask::await_resume() const noexcept
{
}