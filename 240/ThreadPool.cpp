#include "ThreadPool.h"

ThreadPool::Worker::Worker(ThreadPool& pool)
    : pool_(pool)
{
}

void ThreadPool::Worker::operator()()
{
    while (true)
    {
        std::function<void()> f;
        {
            std::unique_lock<std::mutex> lock(pool_.mutex_);
            while (false == pool_.stop_ && pool_.tasks_.empty())
            {
                pool_.condition_.wait(lock);
            }

            if (true == pool_.stop_)
            {
                return;
            }

            f = pool_.tasks_.front();
            pool_.tasks_.pop_front();
        }
        f();
    }
}

ThreadPool::ThreadPool(int thread_count)
    : stop_(false)
{
    for (int i = 0; i < thread_count; i++)
    {
        workers_.push_back(std::thread(Worker(*this)));
    }
}

ThreadPool::~ThreadPool()
{
    stop_ = true;
    condition_.notify_all();

    std::for_each(workers_.begin(), workers_.end(), [](std::thread& thr) {
        thr.join();
        });
}

size_t ThreadPool::GetTaskCount()
{
    std::unique_lock<std::mutex> lock(mutex_);
    return tasks_.size();
}

void ThreadPool::Join()
{
    for (std::thread& thr : workers_) {
        thr.join();
    }
}